using System.Diagnostics;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Unite.Analysis.Configuration.Options;
using Unite.Analysis.Models;
using Unite.Data.Context;
using Unite.Data.Entities.Omics;
using Unite.Essentials.Tsv;

using GeneExpressions = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, int>>; // GeneStableId, SampleId, Reads

namespace Unite.Analysis.Services.De;

public class AnalysisService : AnalysisService<Models.Criteria.Analysis>
{
    private const string _geneIdColumnName = "gene_id";
    private const string _sampleIdColumnName = "sample_id";
    private const string _conditionColumnName = "condition";
    private const string _dataFileName = "data.tsv";
    private const string _metadataFileName = "metadata.tsv";
    private const string _resultsFileName = "results.tsv";
    private const string _resultsFinalFileName = "results_final.tsv";

    private readonly DataLoader _dataLoader;
    private readonly IDbContextFactory<DomainDbContext> _dbContextFactory;


    public AnalysisService(
        IAnalysisOptions options,
        DataLoader dataLoader,
        IDbContextFactory<DomainDbContext> dbContextFactory) : base(options)
    {
        _dataLoader = dataLoader;
        _dbContextFactory = dbContextFactory;
    }


    public override async Task<AnalysisTaskResult> Prepare(Models.Criteria.Analysis model, params object[] args)
    {
        var stopwatch = new Stopwatch();
        var sampleNamesByDataset = new Dictionary<string, string[]>();
        var sampleExpressionsByGene = new Dictionary<string, Dictionary<string, int>>();
        var directoryPath = GetWorkingDirectoryPath(model.Id);

        stopwatch.Restart();

        foreach (var dataset in model.Datasets.OrderBy(dataset => dataset.Order))
        {
            var datasetSampleExpressionsByGene = await _dataLoader.LoadDatasetData(dataset);
            
            var datasetSampleNames = datasetSampleExpressionsByGene.Values
                .SelectMany(geneSampleGroups => geneSampleGroups.Keys)
                .Distinct()
                .ToArray();

            sampleNamesByDataset.Add(dataset.Id, datasetSampleNames);

            foreach (var geneGroup in datasetSampleExpressionsByGene)
            {
                if (!sampleExpressionsByGene.ContainsKey(geneGroup.Key))
                    sampleExpressionsByGene.Add(geneGroup.Key, new Dictionary<string, int>());

                foreach (var sampleGroup in geneGroup.Value)
                {
                    if (!sampleExpressionsByGene[geneGroup.Key].ContainsKey(sampleGroup.Key))
                        sampleExpressionsByGene[geneGroup.Key].Add(sampleGroup.Key, sampleGroup.Value);
                }
            }
        }

        await CreateDataFile(sampleNamesByDataset, sampleExpressionsByGene, directoryPath);
        await CreateMetadataFile(sampleNamesByDataset, sampleExpressionsByGene, directoryPath);

        stopwatch.Stop();

        return AnalysisTaskResult.Success(stopwatch.Elapsed.TotalSeconds);
    }

    public override async Task<AnalysisTaskResult> Process(string key, params object[] args)
    {
        var path = GetWorkingDirectoryPath(key);
        var url = $"{_options.DeUrl}/api/run?key={key}";

        var result = await ProcessRemotely(url);

        if (result.Status == Analysis.Models.Enums.AnalysisTaskStatus.Success)
        {
            var filePath = Path.Join(path, _resultsFileName);
            var filePathFinal = Path.Join(path, _resultsFinalFileName);

            using var dbContext = _dbContextFactory.CreateDbContext();

            var genesMap = dbContext.Set<Gene>()
                .ToDictionary(
                    gene => gene.StableId, 
                    gene => (Id: gene.Id, Symbol: gene.Symbol ?? gene.StableId)
                );

            var mapRaw = new ClassMap<Models.Data.Results>()
                .Map(x => x.GeneStableId, "ID")
                .Map(x => x.Log2FoldChange, "log2FoldChange")
                .Map(x => x.PValueAdjusted, "padj");

            var mapFinal = new ClassMap<Models.Data.Results>()
                .Map(x => x.GeneId, "geneId")
                .Map(x => x.GeneStableId, "geneStableId")
                .Map(x => x.GeneSymbol, "geneSymbol")
                .Map(x => x.Log2FoldChange, "log2FoldChange")
                .Map(x => x.PValueAdjusted, "pValueAdjusted");

            var tsvRaw = await File.ReadAllTextAsync(filePath);

            var dataRaw = TsvReader.Read(tsvRaw, mapRaw);

            var dataFinal = dataRaw.Select(x => new Models.Data.Results
            {
                GeneId = genesMap[x.GeneStableId].Id,
                GeneStableId = x.GeneStableId,
                GeneSymbol = genesMap[x.GeneStableId].Symbol,
                Log2FoldChange = x.Log2FoldChange,
                PValueAdjusted = x.PValueAdjusted
            });

            var tsvFinal = TsvWriter.Write(dataFinal, mapFinal);

            await File.WriteAllTextAsync(filePathFinal, tsvFinal);
        }
        
        return result;
    }

    public override async Task<Stream> Load(string key, params object[] args)
    {
        var path = Path.Join(GetWorkingDirectoryPath(key), _resultsFinalFileName);

        var stream = File.OpenRead(path);

        return await Task.FromResult(stream);
    }

    public override async Task<Stream> Download(string key, params object[] args)
    {
        var path = Path.Join(GetWorkingDirectoryPath(key), _resultsFinalFileName);

        var stream = File.OpenRead(path);

        return await Task.FromResult(stream);
    }

    public override Task Delete(string key, params object[] args)
    {
       var path = GetWorkingDirectoryPath(key);

        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }

        return Task.CompletedTask;
    }


    private static Task CreateDataFile(Dictionary<string, string[]> samplesMap, GeneExpressions expressionsMap, string directoryPath)
    {
        var samples = samplesMap.Values.SelectMany(values => values).Distinct().ToArray();

        var tsv = new StringBuilder();
        tsv.Append($"{_geneIdColumnName}\t");
        tsv.Append(string.Join('\t', samples));
        tsv.Append(Environment.NewLine);

        foreach (var geneId in expressionsMap.Keys)
        {
            var expressions = samples.Select(sampleId => expressionsMap[geneId].TryGetValue(sampleId, out var value) ? value : (int?)null);

            if (ValidateGeneExpressions(expressions))
            {
                tsv.Append($"{geneId}\t");
                tsv.Append(string.Join('\t', expressions));
                tsv.Append(Environment.NewLine);
            }
        }

        var filePath = Path.Join(directoryPath, _dataFileName);
        return File.WriteAllTextAsync(filePath, tsv.ToString());
    }

    private static Task CreateMetadataFile(Dictionary<string, string[]> samplesMap, GeneExpressions expressionsMap, string directoryPath)
    {
        var tsv = new StringBuilder();
        tsv.Append($"{_sampleIdColumnName}\t");
        tsv.Append($"{_conditionColumnName}");
        tsv.Append(Environment.NewLine);

        foreach (var dataset in samplesMap)
        {
            foreach (var sampleId in dataset.Value)
            {
                tsv.AppendLine(string.Join('\t', sampleId, dataset.Key));
            }
        }

        var filePath = Path.Join(directoryPath, _metadataFileName);
        return File.WriteAllTextAsync(filePath, tsv.ToString());
    }

    private static bool ValidateGeneExpressions(IEnumerable<int?> expressions)
    {
        var results = new List<bool>();

        // Should have no null values
        results.Add(expressions.All(expression => expression.HasValue));

        // Should have 50% values greater than 10
        results.Add(expressions.Count(expression => expression > 10) > expressions.Count() / 2);

        return results.All(result => result);
    }
}
