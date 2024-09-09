using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Unite.Analysis.Configuration.Options;
using Unite.Analysis.Helpers;
using Unite.Analysis.Models;
using Unite.Analysis.Services.Rnasc.Models;

namespace Unite.Analysis.Services.Rnasc;

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

        await DataLoader.DownloadResources(context, directoryPath, args[0].ToString());

        WriteOptions(model.Options, directoryPath);

        stopwatch.Stop();

        return AnalysisTaskResult.Success(stopwatch.Elapsed.TotalSeconds);
    }

    public async override Task<AnalysisTaskResult> Process(string key, params object[] args)
    {
        var url = $"{_options.RnascUrl}/api/run?key={key}";

        return await ProcessRemotely(url);
    }

    public async override Task<Stream> Load(string key, params object[] args)
    {
        var path = Path.Combine(GetWorkingDirectoryPath(key), "data.json");

        var stream = new FileStream(path, FileMode.Open);

        return await Task.FromResult(stream);
    }

    public async override Task<Stream> Download(string key, params object[] args)
    {
        var path = Path.Combine(GetWorkingDirectoryPath(key), "data.h5ad");

        var stream = await ArchiveManager.Archive(path, "zip");

        return stream;
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
        var serializerOptions = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumMemberConverter() },
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(options, serializerOptions);

        File.WriteAllText(Path.Combine(path, "options.json"), json);
    }
}
