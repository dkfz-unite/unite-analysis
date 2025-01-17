using System.Diagnostics;
using Unite.Analysis.Configuration.Options;
using Unite.Analysis.Models;
using Unite.Analysis.Models.Enums;

namespace Unite.Analysis.Services.KMeier;

public class AnalysisService : AnalysisService<Models.Criteria.Analysis>
{
    private readonly ContextLoader _contextLoader;

    public AnalysisService(
        IAnalysisOptions options,
        ContextLoader contextLoader) : base(options)
    {
         _contextLoader = contextLoader;
    }
    

    public override async Task<AnalysisTaskResult> Prepare(Models.Criteria.Analysis model, params object[] args)
    {
        var stopwatch = new Stopwatch();

        stopwatch.Restart();

        var path = GetWorkingDirectoryPath(model.Id);

        var context = await _contextLoader.Load(model.Datasets);

        await MetaWriter.Write(context, model.Options, path);

        stopwatch.Stop();

        return AnalysisTaskResult.Success(stopwatch.Elapsed.TotalSeconds);
    }

    public override async Task<AnalysisTaskResult> Process(string key, params object[] args)
    {
        var path = GetWorkingDirectoryPath(key);

        var url = $"{_options.KMeierUrl}/api/run?key={key}";

        var analysisResult = await ProcessRemotely(url);

        if (analysisResult.Status == AnalysisTaskStatus.Success)
        {
            await OutputWriter.ProcessOutput(path);
            await OutputWriter.ArchiveOutput(path);
        }

        return analysisResult;
    }

    public override async Task<Stream> Load(string key, params object[] args)
    {
        var path = Path.Combine(GetWorkingDirectoryPath(key), OutputWriter.OutputFileName);

        var stream = File.OpenRead(path);

        return await Task.FromResult(stream);
    }

    public override async Task<Stream> Download(string key, params object[] args)
    {
        var path = Path.Combine(GetWorkingDirectoryPath(key), OutputWriter.ArchiveFileName);

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
}
