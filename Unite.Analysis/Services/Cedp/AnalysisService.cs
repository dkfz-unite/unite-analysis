using System.Diagnostics;
using System.IO.Compression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Unite.Analysis.Configuration.Options;
using Unite.Analysis.Helpers;
using Unite.Analysis.Models;
using Unite.Analysis.Models.Enums;
using Unite.Analysis.Models.Metadata;
using Unite.Analysis.Models.Structures;
using Unite.Analysis.Services.Dep.Models.Criteria.Enums;
using Unite.Analysis.Services.Cedp.Models.Input;
using Unite.Data.Context;
using Unite.Data.Entities.Omics.Analysis.Enums;
using Unite.Data.Entities.Omics.Analysis.Prot;
using Unite.Essentials.Extensions;
using Unite.Essentials.Tsv;
using Unite.Analysis.Services.Cedp.Models.Criteria.Enums;
using System.Text.Json;

namespace Unite.Analysis.Services.Cedp;

public class AnalysisService : AnalysisService<Models.Criteria.Analysis>
{
    private const string DataFileName = "data.tsv";
    private const string MetadataFileName = "metadata.tsv";
    private const string OptionsFileName = "options.json";
    private const string ResultsFileName = "results.tsv";
    private const string AnnotationsFileName = "annotations.tsv";
    private const string ArchiveFileName = "results.zip";

    private readonly IDbContextFactory<DomainDbContext> _dbContextFactory;
    private readonly SamplesContextLoaderFull _contextLoader;
    private readonly ILogger _logger;


    public AnalysisService(IAnalysisOptions options, IDbContextFactory<DomainDbContext> dbContextFactory, SamplesContextLoaderFull contextLoader, ILogger<AnalysisService> logger) : base(options)
    {
        _dbContextFactory = dbContextFactory;
        _contextLoader = contextLoader;
        _logger = logger;
    }

    public override async Task<AnalysisTaskResult> Prepare(Models.Criteria.Analysis model, params object[] args)
    {
        try
        {
            return await PrepareIn(model, args);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error preparing analysis with ID {AnalysisId}", model.Id);
            return AnalysisTaskResult.Failed(0, ex.Message);
        }
    }

    public async Task<AnalysisTaskResult> PrepareIn(Models.Criteria.Analysis model, params object[] args)
    {
        var stopwatch = Stopwatch.StartNew();

        var directoryPath = GetWorkingDirectoryPath(model.Id);
        var dataFilePath = Path.Combine(directoryPath, DataFileName);
        var metadatapath = Path.Combine(directoryPath, MetadataFileName);
        var annotationsPath = Path.Combine(directoryPath, AnnotationsFileName);
        var optionsPath = Path.Combine(directoryPath, OptionsFileName);

        var data = new Matrix<double>("feature");
        var metadata = new List<MetadataEntry>();

        using var dbContext = _dbContextFactory.CreateDbContext();

        var mappings = new Mappings<SampleMetadata>();
        var dataset = model.Datasets.Single();
        
        using var samplesContext = await _contextLoader.LoadDatasetData(dataset, AnalysisType.MS);
        var samplesMetadata = SampleMetadataLoader.Load(samplesContext);
        var samplesMetadataMap = SampleMetadataMapper.Map(samplesMetadata, mapId: true);
        var sampleIds = samplesContext.OmicsSamples.Keys.ToArray();

        _logger.LogInformation("Loaded sample data for {SampleCount} samples", sampleIds.Length);

        var expressions = await dbContext.Set<ProteinExpression>()
            .AsNoTracking()
            .Include(expression => expression.Entity.Transcript.Gene)
            .Where(expression => sampleIds.Contains(expression.SampleId))
            .ToArrayAsync();

        _logger.LogInformation("Loaded expression data for {ExpressionCount} expressions", expressions.Length);

        _logger.LogInformation("Options {options}\n\r", JsonSerializer.Serialize(model.Options));
        if (model.Options.FeatureType == FeatureType.Gene)
        {
            var expression = expressions.FirstOrDefault(expression => expression.Entity.Transcript.Gene.Symbol.Equals(model.Options.Feature));
            if (expression != null)
                model.Options.Feature = expression.Entity.Transcript.Gene.StableId;
            else
                throw new InvalidOperationException($"There is no expression data for the specified gene {model.Options.Feature}");

            _logger.LogInformation("Mapped feature {Feature} to gene stable ID {StableId}", model.Options.Feature, expression.Entity.Transcript.Gene.StableId);
        }
        else if (model.Options.FeatureType == FeatureType.Protein)
        {
            var expression = expressions.FirstOrDefault(expression => expression.Entity.Symbol.Equals(model.Options.Feature));
            if (expression != null)
                model.Options.Feature = expression.Entity.StableId;
            else
                throw new InvalidOperationException($"There is no expression data for the specified protein {model.Options.Feature}");

            _logger.LogInformation("Mapped feature {Feature} to protein stable ID {StableId}", model.Options.Feature, expression.Entity.StableId);
        }
        else
        {
            _logger.LogError("Unsupported feature type: {FeatureType}", model.Options.FeatureType);
            throw new InvalidOperationException($"Unsupported feature type: {model.Options.FeatureType}");
        }

        foreach (var expression in expressions)
        {
            var column = expression.SampleId.ToString();

            var row = string.Empty;
            if (model.Options.FeatureType == FeatureType.Gene)
                row = expression.Entity.Transcript.Gene.StableId;
            else if (model.Options.FeatureType == FeatureType.Protein)
                row = expression.Entity.StableId;
             else
                throw new InvalidOperationException($"Unsupported feature type: {model.Options.FeatureType}");

            data[column, row] = expression.Raw;
        }

        _logger.LogInformation("Constructed data matrix with {Number} cells", data.ColumnKeys.Count() * data.RowKeys.Count());

        foreach (var sampleId in sampleIds)
        {
            if (!data.ContainsColumn(sampleId.ToString()))
                continue;

            var donor = samplesContext.GetSampleDonor(sampleId);
            var specimen = samplesContext.GetSampleSpecimen(sampleId);
            var sample = samplesContext.OmicsSamples[sampleId];
            var sampleMetadata = samplesMetadata.FirstOrDefault(entry => entry.Key == sampleId);
            var conditionMapping = mappings.All.FirstOrDefault(mapping => mapping.Key == model.Options.ConditionProperty);
            var conditionGetter = conditionMapping?.Expression.Compile();
            var condition = conditionGetter?.Invoke(sampleMetadata);

            metadata.Add(new MetadataEntry
            {
                Sample = sampleId,
                Condition = condition,
                Batch = sample.Batch,
                Donor = donor.ReferenceId,
                Specimen = specimen.ReferenceId,
                SpecimenType = specimen.TypeId.ToDefinitionString()
            });
        }

        _logger.LogInformation("Constructed metadata for {SampleCount} samples", metadata.Count);

        var batchValidationError = ValidateBatches(model.Options.BatchCorrectionMethod, metadata);

        data.WriteTo(dataFilePath);
        File.WriteAllText(metadatapath, TsvWriter.Write(metadata));
        File.WriteAllText(annotationsPath, TsvWriter.Write(samplesMetadata, samplesMetadataMap));
        MemberJsonSerializer.Serialize(optionsPath, model.Options);

        _logger.LogInformation("Written data, metadata, annotations and options to disk");

        stopwatch.Stop();

        return AnalysisTaskResult.Success(stopwatch.Elapsed.TotalSeconds, batchValidationError);
    }

    public override async Task<AnalysisTaskResult> Process(string key, params object[] args)
    {
        var path = GetWorkingDirectoryPath(key);

        var url = $"{_options.CedpUrl}/api/run?key={key}";

        var analysisResult = await ProcessRemotely(url);

        if (analysisResult.Status == AnalysisTaskStatus.Success)
        {
            ArchiveResults(path);
        }

        return analysisResult;
    }

    public override async Task<Stream> Load(string key, params object[] args)
    {
        var file = args.IsNotEmpty() && args[0] != null ? args[0].ToString() : ResultsFileName; 
            
        var path = Path.Combine(GetWorkingDirectoryPath(key), file);

        var stream = File.OpenRead(path);

        return await Task.FromResult(stream);
    }

    public override async Task<Stream> Download(string key, params object[] args)
    {
        var path = Path.Combine(GetWorkingDirectoryPath(key), ArchiveFileName);

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


    private static string ValidateBatches(BatchCorrectionMethod? method, IEnumerable<MetadataEntry> metadata)
    {
        if (method == null)
            return null;

        var somehaveBatches = metadata.Any(entry => !string.IsNullOrEmpty(entry.Batch));
        var allhaveBatches = metadata.All(entry => !string.IsNullOrEmpty(entry.Batch));

        if (somehaveBatches && !allhaveBatches)
            return "Some samples do not have batch information. Batch correction could not be provied.";
        
        return null;
    }
    
    private static void ArchiveResults(string path)
    {
        using var archiveStream = new FileStream(Path.Combine(path, ArchiveFileName), FileMode.CreateNew);
        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, false);

        archive.CreateEntryFromFile(Path.Combine(path, DataFileName), DataFileName);
        archive.CreateEntryFromFile(Path.Combine(path, MetadataFileName), MetadataFileName);
        archive.CreateEntryFromFile(Path.Combine(path, AnnotationsFileName), AnnotationsFileName);
        archive.CreateEntryFromFile(Path.Combine(path, OptionsFileName), OptionsFileName);
        archive.CreateEntryFromFile(Path.Combine(path, ResultsFileName), ResultsFileName);
    }
}
