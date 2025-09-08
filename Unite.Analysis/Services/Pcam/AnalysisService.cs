using System.Diagnostics;
using Unite.Analysis.Configuration.Options;
using Unite.Analysis.Helpers;
using Unite.Analysis.Models;
using Unite.Analysis.Models.Enums;
using Unite.Analysis.Services.Pcam.Models.Criteria;
using Unite.Data.Entities.Omics.Analysis.Enums;

namespace Unite.Analysis.Services.Pcam;

public class AnalysisService : AnalysisService<Models.Criteria.Analysis>
{
    private readonly ContextLoader _contextLoader;


    public AnalysisService(IAnalysisOptions options, ContextLoader contextLoader) : base(options)
    {
        _contextLoader = contextLoader;
    }


    public override async Task<AnalysisTaskResult> Prepare(Models.Criteria.Analysis model, params object[] args)
    {
        var stopwatch = new Stopwatch();

        stopwatch.Restart();

        var directoryPath = GetWorkingDirectoryPath(model.Id);

        var dataset = model.Datasets.Single();

        var context = await _contextLoader.LoadDatasetData(dataset, AnalysisType.MethArray);
        await DataLoader.DownloadResources(context, directoryPath, args[0].ToString(), _options.DataHost);
        await MetaLoader.PrepareMetadata(context, directoryPath);

        WriteOptions(model.Options, directoryPath);

        stopwatch.Stop();

        return AnalysisTaskResult.Success(stopwatch.Elapsed.TotalSeconds);
    }

    public async override Task<AnalysisTaskResult> Process(string key, params object[] args)
    {
        var path = GetWorkingDirectoryPath(key);

        var url = $"{_options.PcamUrl}/api/run?key={key}";

        var analysisResult = await ProcessRemotely(url);

        if (analysisResult.Status == AnalysisTaskStatus.Success)
        {
            await OutputWriter.ProcessOutput(path);
            await OutputWriter.ArchiveOutput(path);
        }

        return analysisResult;
    }

    public async override Task<Stream> Load(string key, params object[] args)
    {
        var path = Path.Combine(GetWorkingDirectoryPath(key), OutputWriter.ResultsFileName);;
        if (args[0] != null)
        {
            path = Path.Combine(GetWorkingDirectoryPath(key), OutputWriter.MetadataFileName);
        }

        var stream = File.OpenRead(path);

        return await Task.FromResult(stream);
    }

    public async override Task<Stream> Download(string key, params object[] args)
    {
        var path = Path.Combine(GetWorkingDirectoryPath(key), OutputWriter.ArchiveFileName);

        var stream = File.OpenRead(path);

        return await Task.FromResult(stream);
    }

    public override Task Delete(string key, params object[] args)
    {
        var directoryPath = Path.Combine(_options.DataPath, key);

        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, true);
        }

        return Task.CompletedTask;
    }

    private static void WriteOptions(Options options, string path)
    {
        var json = MemberJsonSerializer.Serialize(options);

        File.WriteAllText(Path.Combine(path, "options.json"), json);
    }
}
