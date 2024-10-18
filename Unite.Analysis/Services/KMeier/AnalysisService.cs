using System.Diagnostics;
using Unite.Analysis.Configuration.Options;
using Unite.Analysis.Models;

namespace Unite.Analysis.Services.KMeier;

public class AnalysisService : AnalysisService<Models.Analysis>
{
    private readonly ContextLoader _contextLoader;

    public AnalysisService(
        IAnalysisOptions options,
        ContextLoader contextLoader) : base(options)
    {
         _contextLoader = contextLoader;
    }
    

    public override async Task<AnalysisTaskResult> Prepare(Models.Analysis model, params object[] args)
    {
        var stopwatch = new Stopwatch();

        stopwatch.Restart();

        var directoryPath = GetWorkingDirectoryPath(model.Key);

        var context = await _contextLoader.LoadDatasetData(model.Datasets.SingleOrDefault());

        await MetaLoader.PrepareMetadata(context, directoryPath);

        stopwatch.Stop();

        return AnalysisTaskResult.Success(stopwatch.Elapsed.TotalSeconds);
    }

    public override async Task<AnalysisTaskResult> Process(string key, params object[] args)
    {
        var url = $"{_options.KMeierUrl}/api/run?key={key}";

        await ProcessRemotely(url);

        return AnalysisTaskResult.Success();
    }

    public override async Task<Stream> Load(string key, params object[] args)
    {
        var path = Path.Combine(GetWorkingDirectoryPath(key), "result.tsv");

        var stream = File.OpenRead(path);

        return await Task.FromResult(stream);
    }

    public override async Task<Stream> Download(string key, params object[] args)
    {
        var path = Path.Combine(GetWorkingDirectoryPath(key), "result.tsv");

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
}
