using System.Diagnostics;
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

namespace Unite.Analysis.Services.Cedp;

public class AnalysisService : AnalysisService<Models.Criteria.Analysis>
{
    private readonly IDbContextFactory<DomainDbContext> _dbContextFactory;
    private readonly SamplesContextLoaderFull _contextLoader;
    private readonly ILogger _logger;

    public static readonly string DataFileName = InputFile("data.tsv");
    public static readonly string MetadataFileName = InputFile ("metadata.tsv");
    public static readonly string SamplesFileName = InputFile("samples.tsv");
    public static readonly string ValuesFileName = OutputFile("values.tsv");
    public static readonly string ContrastsFileName = OutputFile("contrasts.tsv");
    public override string DefaultLoadFileName => ValuesFileName;


    public AnalysisService(
        IAnalysisOptions options,
        IDbContextFactory<DomainDbContext> dbContextFactory,
        SamplesContextLoaderFull contextLoader,
        ILogger<AnalysisService> logger) : base(options)
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

        var optionsPath = Path.Combine(directoryPath, OptionsFileName);
        var dataFilePath = Path.Combine(directoryPath, DataFileName);
        var metadatapath = Path.Combine(directoryPath, MetadataFileName);
        var samplesPath = Path.Combine(directoryPath, SamplesFileName);
        

        var data = new Matrix<double>("feature");
        var metadata = new List<MetadataEntry>();
        
        using var dbContext = _dbContextFactory.CreateDbContext();

        var mappings = new MetadataMappings<SampleMetadata>();
        var dataset = model.Datasets.Single();
        
        using var samplesContext = await _contextLoader.LoadDatasetData(dataset, AnalysisType.MS);
        var samplesMetadata = SampleMetadataLoader.Load(samplesContext);
        var samplesMetadataMap = SampleMetadataMapper.Map(samplesMetadata, mapId: true);
        var sampleIds = new List<int>();

        foreach (var sampleId in samplesContext.OmicsSamples.Keys.ToArray())
        {
            var donor = samplesContext.GetSampleDonor(sampleId);
            var specimen = samplesContext.GetSampleSpecimen(sampleId);
            var sample = samplesContext.OmicsSamples[sampleId];
            var sampleMetadata = samplesMetadata.FirstOrDefault(entry => entry.Key == sampleId);
            var conditionMapping = mappings.All.FirstOrDefault(mapping => mapping.Key == model.Options.ConditionProperty);
            var conditionGetter = conditionMapping?.Expression.Compile();
            var condition = conditionGetter?.Invoke(sampleMetadata);

            if (model.Options.ConditionValue?.Length > 0 && !model.Options.ConditionValue.Contains(condition))
                continue;

            metadata.Add(new MetadataEntry
            {
                Sample = sampleId,
                Condition = condition,
                Batch = sample.Batch,
                Donor = donor.ReferenceId,
                Specimen = specimen.ReferenceId,
                SpecimenType = specimen.TypeId.ToDefinitionString()
            });

            sampleIds.Add(sampleId);
        }

        var expressions = await dbContext.Set<ProteinExpression>()
            .AsNoTracking()
            .Include(expression => expression.Entity.Transcript.Gene)
            .Where(expression => sampleIds.Contains(expression.SampleId))
            .ToArrayAsync();

        if (model.Options.FeatureType == FeatureType.Gene)
        {
            var expression = expressions.FirstOrDefault(expression => string.Equals(expression.Entity.Transcript.Gene.Symbol, model.Options.FeatureName));
            if (expression != null)
                model.Options.Feature = expression.Entity.Transcript.Gene.StableId;
            else
                throw new InvalidOperationException($"There is no expression data for the specified gene {model.Options.FeatureName}");
        }
        else if (model.Options.FeatureType == FeatureType.Protein)
        {
            var expression = expressions.FirstOrDefault(expression => string.Equals(expression.Entity.Symbol, model.Options.FeatureName));
            if (expression != null)
                model.Options.Feature = expression.Entity.StableId;
            else
                throw new InvalidOperationException($"There is no expression data for the specified protein {model.Options.FeatureName}");
        }
        else
        {
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

        foreach (var sampleId in sampleIds)
        {
            if (!data.ContainsColumn(sampleId.ToString()))
                metadata.Remove(metadata.First(entry => entry.Sample == sampleId));
        }

        var batchValidationError = ValidateBatches(model.Options.BatchCorrectionMethod, metadata);

        data.WriteTo(dataFilePath);
        File.WriteAllText(metadatapath, TsvWriter.Write(metadata));
        File.WriteAllText(samplesPath, TsvWriter.Write(samplesMetadata, samplesMetadataMap));
        MemberJsonSerializer.Serialize(optionsPath, model.Options);

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
}
