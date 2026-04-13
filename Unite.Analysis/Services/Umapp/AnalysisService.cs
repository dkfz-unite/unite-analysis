using System.Diagnostics;
using System.IO.Compression;
using Microsoft.EntityFrameworkCore;
using Unite.Analysis.Configuration.Options;
using Unite.Analysis.Helpers;
using Unite.Analysis.Models;
using Unite.Analysis.Models.Enums;
using Unite.Analysis.Models.Structures;
using Unite.Analysis.Services.Dep.Models.Criteria.Enums;
using Unite.Analysis.Services.Umapp.Models.Input;
using Unite.Data.Entities.Omics.Analysis.Enums;
using Unite.Data.Entities.Omics.Analysis.Prot;
using Unite.Essentials.Extensions;
using Unite.Essentials.Tsv;

namespace Unite.Analysis.Services.Umapp;

public class AnalysisService : AnalysisService<Models.Criteria.Analysis>
{
    private const string DataFileName = "data.tsv";
    private const string MetadataFileName = "metadata.tsv";
    private const string OptionsFileName = "options.json";
    private const string ResultsFileName = "results.tsv";
    private const string AnnotationsFileName = "annotations.tsv";
    private const string ArchiveFileName = "results.zip";

    private readonly SamplesContextLoaderFull _contextLoader;


    public AnalysisService(IAnalysisOptions options, SamplesContextLoaderFull contextLoader) : base(options)
    {
        _contextLoader = contextLoader;
    }


    public override async Task<AnalysisTaskResult> Prepare(Models.Criteria.Analysis model, params object[] args)
    {
        var stopwatch = Stopwatch.StartNew();

        var directoryPath = GetWorkingDirectoryPath(model.Id);
        var dataFilePath = Path.Combine(directoryPath, DataFileName);
        var metadatapath = Path.Combine(directoryPath, MetadataFileName);
        var annotationsPath = Path.Combine(directoryPath, AnnotationsFileName);
        var optionsPath = Path.Combine(directoryPath, OptionsFileName);

        var data = new Matrix<double>("feature");
        var metadata = new List<MetadataEntry>();

        using var dbContext = _contextLoader.DbContextFactory.CreateDbContext();

        var dataset = model.Datasets.Single();
        var samplesContext = await _contextLoader.LoadDatasetData(dataset, AnalysisType.MS);
        var samplesMetadata = SampleMetadataLoader.Load(samplesContext);
        var samplesMetadataMap = SampleMetadataMapper.Map(samplesMetadata, mapId: true);
        var sampleIds = samplesContext.OmicsSamples.Keys.ToArray();

        var expressions = await dbContext.Set<ProteinExpression>()
            .AsNoTracking()
            .Include(expression => expression.Entity.Transcript.Gene)
            .Where(expression => sampleIds.Contains(expression.SampleId))
            .ToArrayAsync();

        foreach (var expression in expressions)
        {
            data[expression.SampleId.ToString(), expression.Entity.StableId] = expression.Raw;
        }

        foreach (var sampleId in sampleIds)
        {
            if (!data.ContainsColumn(sampleId.ToString()))
                continue;

            var donor = samplesContext.GetSampleDonor(sampleId);
            var specimen = samplesContext.GetSampleSpecimen(sampleId);
            var sample = samplesContext.OmicsSamples[sampleId];

            metadata.Add(new MetadataEntry
            {
                Sample = sampleId,
                Batch = sample.Batch,
                Donor = donor.ReferenceId,
                Specimen = specimen.ReferenceId,
                SpecimenType = specimen.TypeId.ToDefinitionString()
            });
        }

        var batchValidationError = ValidateBatches(model.Options.BatchCorrectionMethod, metadata);

        data.WriteTo(dataFilePath);
        File.WriteAllText(metadatapath, TsvWriter.Write(metadata));
        File.WriteAllText(annotationsPath, TsvWriter.Write(samplesMetadata, samplesMetadataMap));
        MemberJsonSerializer.Serialize(optionsPath, model.Options);

        stopwatch.Stop();

        return AnalysisTaskResult.Success(stopwatch.Elapsed.TotalSeconds, batchValidationError);
    }

    public override async Task<AnalysisTaskResult> Process(string key, params object[] args)
    {
        var path = GetWorkingDirectoryPath(key);

        var url = $"{_options.UmappUrl}/api/run?key={key}";

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
