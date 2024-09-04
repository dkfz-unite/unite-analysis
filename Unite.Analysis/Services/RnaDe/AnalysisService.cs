using System.Diagnostics;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Unite.Analysis.Configuration.Options;
using Unite.Analysis.Models;
using Unite.Data.Context;
using Unite.Data.Entities.Genome;
using Unite.Essentials.Tsv;

using GeneExpressions = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, int>>; // GeneStableId, SampleId, Reads

namespace Unite.Analysis.Services.RnaDe;

public class AnalysisService : AnalysisService<Models.Analysis, string>
{
    private const string _geneIdColumnName = "gene_id";
    private const string _sampleIdColumnName = "sample_id";
    private const string _conditionColumnName = "condition";
    private const string _dataFileNameTemplate = "{0}_data.tsv";
    private const string _metadataFileNameTemplate = "{0}_metadata.tsv";
    private const string _resultsFileNameTemplate = "{0}_results.tsv";
    private const string _resultsFinalFileNameTemplate = "{0}_results_final.tsv";

    private readonly IAnalysisOptions _options;
    private readonly DataLoader _dataLoader;
    private readonly IDbContextFactory<DomainDbContext> _dbContextFactory;


    public AnalysisService(
        IAnalysisOptions options,
        DataLoader dataLoader,
        IDbContextFactory<DomainDbContext> dbContextFactory)
    {
        _options = options;
        _dataLoader = dataLoader;
        _dbContextFactory = dbContextFactory;
    }


    public override async Task<AnalysisTaskResult> Prepare(Models.Analysis model, params object[] args)
    {
        var stopwatch = new Stopwatch();
        var sampleNamesByDataset = new Dictionary<string, string[]>();
        var sampleExpressionsByGene = new Dictionary<string, Dictionary<string, int>>();

        stopwatch.Restart();

        foreach (var dataset in model.Datasets.OrderBy(dataset => dataset.Order))
        {
            var datasetSampleExpressionsByGene = await _dataLoader.LoadDatasetData(dataset);
            
            var datasetSampleNames = datasetSampleExpressionsByGene.Values
                .SelectMany(geneSampleGroups => geneSampleGroups.Keys)
                .Distinct()
                .ToArray();

            sampleNamesByDataset.Add(dataset.Key, datasetSampleNames);

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

        await CreateDataFile(sampleNamesByDataset, sampleExpressionsByGene, model.Key);
        await CreateMetadataFile(sampleNamesByDataset, sampleExpressionsByGene, model.Key);

        stopwatch.Stop();

        return AnalysisTaskResult.Success(stopwatch.Elapsed.TotalSeconds);
    }

    public override Task<AnalysisTaskResult> Process(string key, params object[] args)
    {
        var url = $"{_options.RnaDeUrl}/api/run?key={key}";

        return ProcessRemotely(url);
    }

    public override async Task<string> Load(string key, params object[] args)
    {
        var fileName = string.Format(_resultsFileNameTemplate, key);
        var filePath = Path.Join(_options.DataPath, fileName);

        var fileNameFinal = string.Format(_resultsFinalFileNameTemplate, key);
        var filePathFinal = Path.Join(_options.DataPath, fileNameFinal);

        if (File.Exists(filePathFinal))
        {
            // Read the final results
            return await File.ReadAllTextAsync(filePathFinal);   
        }
        else if (File.Exists(filePath))
        {
            // Compress the results, save and return them
            using var dbContext = _dbContextFactory.CreateDbContext();

            var genesMap = dbContext.Set<Gene>()
                .ToDictionary(
                    gene => gene.StableId, 
                    gene => (Id: gene.Id, Symbol: gene.Symbol ?? gene.StableId)
                );

            var mapRaw = new ClassMap<Models.Results>()
                .Map(x => x.GeneStableId, "ID")
                .Map(x => x.Log2FoldChange, "log2FoldChange")
                .Map(x => x.PValueAdjusted, "padj");

            var mapFinal = new ClassMap<Models.Results>()
                .Map(x => x.GeneId, "geneId")
                .Map(x => x.GeneStableId, "geneStableId")
                .Map(x => x.GeneSymbol, "geneSymbol")
                .Map(x => x.Log2FoldChange, "log2FoldChange")
                .Map(x => x.PValueAdjusted, "pValueAdjusted");

            var tsvRaw = await File.ReadAllTextAsync(filePath);

            var dataRaw = TsvReader.Read(tsvRaw, mapRaw);

            var dataFinal = dataRaw.Select(x => new Models.Results
            {
                GeneId = genesMap[x.GeneStableId].Id,
                GeneStableId = x.GeneStableId,
                GeneSymbol = genesMap[x.GeneStableId].Symbol,
                Log2FoldChange = x.Log2FoldChange,
                PValueAdjusted = x.PValueAdjusted
            });

            var tsvFinal = TsvWriter.Write(dataFinal, mapFinal);

            await File.WriteAllTextAsync(filePathFinal, tsvFinal);

            return tsvFinal;
        }
        else
        {
            // Return no results
            return null;
        }
    }

    public override Task<string> Download(string key, params object[] args)
    {
        var fileNameFinal = string.Format(_resultsFinalFileNameTemplate, key);
        var filePathFinal = Path.Join(_options.DataPath, fileNameFinal);
        if (File.Exists(filePathFinal))
            return File.ReadAllTextAsync(filePathFinal);

        return null;        
    }

    public override Task Delete(string key, params object[] args)
    {
        var dataFileName = string.Format(_dataFileNameTemplate, key);
        var dataFilePath = Path.Join(_options.DataPath, dataFileName);
        if (File.Exists(dataFilePath))
            File.Delete(dataFilePath);

        var metadataFileName = string.Format(_metadataFileNameTemplate, key);
        var metadataFilePath = Path.Join(_options.DataPath, metadataFileName);
        if (File.Exists(metadataFilePath))
            File.Delete(metadataFilePath);

        var resultsFileName = string.Format(_resultsFileNameTemplate, key);
        var resultsFilePath = Path.Join(_options.DataPath, resultsFileName);
        if (File.Exists(resultsFilePath))
            File.Delete(resultsFilePath);

        var resultsFinalFileName = string.Format(_resultsFinalFileNameTemplate, key);
        var resultsFinalFilePath = Path.Join(_options.DataPath, resultsFinalFileName);
        if (File.Exists(resultsFinalFilePath))
            File.Delete(resultsFinalFilePath);

        return Task.CompletedTask;
    }


    private Task CreateDataFile(Dictionary<string, string[]> samplesMap, GeneExpressions expressionsMap, string key)
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

        var fileName = string.Format(_dataFileNameTemplate, key);
        var filePath = Path.Join(_options.DataPath, fileName);
        return File.WriteAllTextAsync(filePath, tsv.ToString());
    }

    private Task CreateMetadataFile(Dictionary<string, string[]> samplesMap, GeneExpressions expressionsMap, string key)
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

        var fileName = string.Format(_metadataFileNameTemplate, key);
        var filePath = Path.Join(_options.DataPath, fileName);
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
