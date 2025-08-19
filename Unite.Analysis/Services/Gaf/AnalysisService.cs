using System.Diagnostics;
using System.IO.Compression;
using Unite.Analysis.Configuration.Options;
using Unite.Analysis.Helpers;
using Unite.Analysis.Models;
using Unite.Analysis.Services.Gaf.Models.Criteria;
using Unite.Analysis.Services.Gaf.Models.Output;
using Unite.Data.Entities.Omics.Analysis.Enums;

namespace Unite.Analysis.Services.Gaf;

public class AnalysisService : AnalysisService<Models.Criteria.Analysis>
{
    private const string OptionsFileName = "options.json";
    private const string ResultFileName = "result.json";
    private const string ArchiveFileName = "output.zip";


    private readonly ContextLoader _contextLoader;
    private readonly ProcessingService _processingService;

    public AnalysisService(
        IAnalysisOptions options,
        ContextLoader contextLoader,
        ProcessingService processingService) : base(options)
    {
        _contextLoader = contextLoader;
        _processingService = processingService;
    }


    public override async Task<AnalysisTaskResult> Prepare(Models.Criteria.Analysis model, params object[] args)
    {
        var stopwatch = Stopwatch.StartNew();

        var directoryPath = GetWorkingDirectoryPath(model.Id);

        var context = await _contextLoader.LoadDatasetData(model.Datasets.SingleOrDefault(), [AnalysisType.WGS, AnalysisType.WES]);

        WriteOptions(model.Options, directoryPath);

        var records = _processingService.ProcessData(context, model.Options);

        WriteRecords(records, directoryPath);

        stopwatch.Stop();

        return AnalysisTaskResult.Success(stopwatch.Elapsed.TotalSeconds);
    }

    public override async Task<AnalysisTaskResult> Process(string key, params object[] args)
    {
        var stopwatch = Stopwatch.StartNew();

        var directoryPath = GetWorkingDirectoryPath(key);

        using var archiveStream = new FileStream(Path.Combine(directoryPath, ArchiveFileName), FileMode.CreateNew);
        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, false);

        archive.CreateEntryFromFile(Path.Combine(directoryPath, ResultFileName), ResultFileName);
        archive.CreateEntryFromFile(Path.Combine(directoryPath, OptionsFileName), OptionsFileName);

        await Task.CompletedTask;

        stopwatch.Stop();

        return AnalysisTaskResult.Success(stopwatch.Elapsed.TotalSeconds);
    }

    public override async Task<Stream> Load(string key, params object[] args)
    {
        var path = Path.Combine(GetWorkingDirectoryPath(key), ResultFileName);

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


    private static void WriteOptions(Options options, string path)
    {
        var json = MemberJsonSerializer.Serialize(options);

        File.WriteAllText(Path.Combine(path, OptionsFileName), json);
    }

    private static void WriteRecords(Records records, string path)
    {
        var json = MemberJsonSerializer.Serialize(records);

        File.WriteAllText(Path.Combine(path, ResultFileName), json);
    }
}
