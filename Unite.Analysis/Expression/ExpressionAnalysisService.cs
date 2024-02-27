using System.Diagnostics;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Unite.Analysis.Configuration.Options;
using Unite.Analysis.Expression.Extensions; 
using Unite.Analysis.Expression.Models;
using Unite.Analysis.Models;
using Unite.Analysis.Models.Enums;
using Unite.Data.Context;
using Unite.Data.Context.Repositories;
using Unite.Data.Entities.Genome;
using Unite.Data.Entities.Genome.Analysis;
using Unite.Data.Entities.Genome.Transcriptomics;
using Unite.Essentials.Tsv;
using Unite.Indices.Search.Services;

using DonorIndex = Unite.Indices.Entities.Donors.DonorIndex;
using ImageIndex = Unite.Indices.Entities.Images.ImageIndex;
using SpecimenIndex = Unite.Indices.Entities.Specimens.SpecimenIndex;

using GeneExpressions = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, int>>; // GeneStableId, SampleId, Reads
using SampleExpressions = (int, Unite.Data.Entities.Genome.Transcriptomics.BulkExpression[]); // SampleId, BulkExpression[]

namespace Unite.Analysis.Expression;

public class ExpressionAnalysisService : AnalysisService<Models.Analysis, string>
{
    private readonly IAnalysisOptions _options;
    private readonly ISearchService<DonorIndex> _donorsSearchService;
    private readonly ISearchService<ImageIndex> _imagesSearchService;
    private readonly ISearchService<SpecimenIndex> _specimensSearchService;
    private readonly IDbContextFactory<DomainDbContext> _dbContextFactory;

    private const byte _threadsCount = 10;
    private const string _geneIdColumnName = "gene_id";
    private const string _sampleIdColumnName = "sample_id";
    private const string _conditionColumnName = "condition";
    private const string _dataFileNameTemplate = "{0}_data.tsv";
    private const string _metadataFileNameTemplate = "{0}_metadata.tsv";
    private const string _resultsFileNameTemplate = "{0}_results.tsv";
    private const string _resultsFinalFileNameTemplate = "{0}_results_final.tsv";

    public ExpressionAnalysisService(
        IAnalysisOptions options,
        ISearchService<DonorIndex> donorsSearchService,
        ISearchService<ImageIndex> imagesSearchService,
        ISearchService<SpecimenIndex> specimensSearchService,
        IDbContextFactory<DomainDbContext> dbContextFactory)
    {
        _options = options;
        _donorsSearchService = donorsSearchService;
        _imagesSearchService = imagesSearchService;
        _specimensSearchService = specimensSearchService;
        _dbContextFactory = dbContextFactory;
    }


    public override async Task<AnalysisTaskResult> Prepare(Models.Analysis model)
    {
        var stopwatch = new Stopwatch();
        var sampleNamesByCohort = new Dictionary<string, string[]>();
        var sampleExpressionsByGene = new Dictionary<string, Dictionary<string, int>>();

        stopwatch.Restart();

        foreach (var cohort in model.Cohorts.OrderBy(cohort => cohort.Order))
        {
            var cohortSampleExpressionsByGene = await LoadDatasetData(cohort);
            
            var cohortSampleNames = cohortSampleExpressionsByGene.Values
                .SelectMany(geneSampleGroups => geneSampleGroups.Keys)
                .Distinct()
                .ToArray();

            sampleNamesByCohort.Add(cohort.Key, cohortSampleNames);

            foreach (var geneGroup in cohortSampleExpressionsByGene)
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

        await CreateDataFile(sampleNamesByCohort, sampleExpressionsByGene, model.Key);
        await CreateMetadataFile(sampleNamesByCohort, sampleExpressionsByGene, model.Key);

        stopwatch.Stop();

        return AnalysisTaskResult.Success(stopwatch.Elapsed.TotalSeconds);
    }

    public override Task<AnalysisTaskResult> Process(string key)
    {
        var url = $"{_options.DESeq2Url}/api/run?key={key}";

        return ProcessRemotely(url);
    }

    public override async Task<string> LoadResult(string key)
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

            var mapRaw = new ClassMap<AnalysisResults>()
                .Map(x => x.GeneStableId, "ID")
                .Map(x => x.Log2FoldChange, "log2FoldChange")
                .Map(x => x.PValueAdjusted, "padj");

            var mapFinal = new ClassMap<AnalysisResults>()
                .Map(x => x.GeneId, "geneId")
                .Map(x => x.GeneStableId, "geneStableId")
                .Map(x => x.GeneSymbol, "geneSymbol")
                .Map(x => x.Log2FoldChange, "log2FoldChange")
                .Map(x => x.PValueAdjusted, "pValueAdjusted");

            var tsvRaw = await File.ReadAllTextAsync(filePath);

            var dataRaw = TsvReader.Read(tsvRaw, mapRaw);

            var dataFinal = dataRaw.Select(x => new AnalysisResults
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

    public override Task<string> DownloadResult(string key)
    {
        var fileNameFinal = string.Format(_resultsFinalFileNameTemplate, key);
        var filePathFinal = Path.Join(_options.DataPath, fileNameFinal);
        if (File.Exists(filePathFinal))
            return File.ReadAllTextAsync(filePathFinal);

        return null;        
    }

    public override Task DeleteData(string key)
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

        foreach (var cohort in samplesMap)
        {
            foreach (var sampleId in cohort.Value)
            {
                tsv.AppendLine(string.Join('\t', sampleId, cohort.Key));
            }
        }

        var fileName = string.Format(_metadataFileNameTemplate, key);
        var filePath = Path.Join(_options.DataPath, fileName);
        return File.WriteAllTextAsync(filePath, tsv.ToString());
    }

    private Task<GeneExpressions> LoadDatasetData(DatasetCriteria model)
    {
        return model.Domain switch
        {
            DatasetDomain.Donors => LoadDonorsDatasetData(model),
            DatasetDomain.Mris => LoadImagesDatasetData(model),
            DatasetDomain.Materials => LoadSpecimensDatasetData(model),
            DatasetDomain.Lines => LoadSpecimensDatasetData(model),
            DatasetDomain.Organoids => LoadSpecimensDatasetData(model),
            DatasetDomain.Xenografts => LoadSpecimensDatasetData(model),
            _ => throw new NotSupportedException()
        };
    }

    private async Task<GeneExpressions> LoadDonorsDatasetData(DatasetCriteria model)
    {
        var stats = await _donorsSearchService.Stats(model.Criteria);

        var ids = stats.Keys.Cast<int>().ToArray();

        var expressions = new GeneExpressions();

        await ids.Batch(_threadsCount, id => 
            LoadDonorExpressions(id)
                .ContinueWith(task => MapSampleExpressions(model.Domain, task.Result, expressions))
        );

        return expressions;
    }

    private async Task<GeneExpressions> LoadImagesDatasetData(DatasetCriteria model)
    {
        var stats = await _imagesSearchService.Stats(model.Criteria);

        var ids = stats.Keys.Cast<int>().ToArray();

        var expressions = new GeneExpressions();

        await ids.Batch(_threadsCount, id => 
            LoadImageExpressions(id)
                .ContinueWith(task => MapSampleExpressions(model.Domain, task.Result, expressions))
        );

        return expressions;
    }

    private async Task<GeneExpressions> LoadSpecimensDatasetData(DatasetCriteria model)
    {
        var stats = await _specimensSearchService.Stats(model.Criteria);

        var ids = stats.Keys.Cast<int>().ToArray();

        var expressions = new GeneExpressions();

        await ids.Batch(_threadsCount, id => 
            LoadSpecimenExpressions(id)
                .ContinueWith(task => MapSampleExpressions(model.Domain, task.Result, expressions))
        );

        return expressions;
    }

    private async Task<SampleExpressions> LoadDonorExpressions(int id)
    {
        var donorRepository = new DonorsRepository(_dbContextFactory);

        var specimenIds = await donorRepository.GetRelatedSpecimens([id]);

        var expressions = await LoadSampleExpressions(specimenIds);

        return (id, expressions);
    }

    private async Task<SampleExpressions> LoadImageExpressions(int id)
    {
        var imageRepository = new ImagesRepository(_dbContextFactory);

        var specimenIds = await imageRepository.GetRelatedSpecimens([id]);

        var expressions = await LoadSampleExpressions(specimenIds);

        return (id, expressions);
    }

    private async Task<SampleExpressions> LoadSpecimenExpressions(int id)
    {
        var expressions = await LoadSampleExpressions([id]);

        return (id, expressions);
    }

    private async Task<BulkExpression[]> LoadSampleExpressions(int[] specimenIds)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var analysedSample = await dbContext.Set<AnalysedSample>()
            .AsNoTracking()
            .Where(sample => specimenIds.Contains(sample.TargetSampleId))
            .Where(sample => sample.BulkExpressions.Any())
            .PickOrDefaultAsync();

        if (analysedSample != null)
        {
            return await dbContext.Set<BulkExpression>()
                .AsNoTracking()
                .Include(expression => expression.Entity)
                .Where(expression => expression.AnalysedSampleId == analysedSample.Id)
                .OrderBy(expression => expression.Entity.ChromosomeId)
                .ThenBy(expression => expression.Entity.Start)
                .ToArrayAsync();
        }
        else
        {
            return null;
        }
        
    }


    private static void MapSampleExpressions(DatasetDomain domain, SampleExpressions sampleExpressions, GeneExpressions geneExpressions)
    {
        var (id, expressions) = sampleExpressions;

        if (expressions != null)
        {
            foreach (var expression in expressions)
            {
                var geneId = expression.Entity.StableId;

                lock (geneExpressions)
                {
                    if (!geneExpressions.ContainsKey(geneId))
                        geneExpressions.Add(geneId, []);

                    geneExpressions[geneId].Add($"{domain}_{id}", expression.Reads);
                }
            }
        }
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
