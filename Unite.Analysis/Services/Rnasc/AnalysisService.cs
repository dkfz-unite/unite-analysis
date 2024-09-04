using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Unite.Analysis.Configuration.Options;
using Unite.Analysis.Helpers;
using Unite.Analysis.Models;
using Unite.Analysis.Services.Rnasc.Models;

namespace Unite.Analysis.Services.Rnasc;

public class AnalysisService : AnalysisService<Models.Analysis, Stream>
{
    private readonly IAnalysisOptions _options;
    private readonly ContextLoader _contextLoader;


    public AnalysisService(
        IAnalysisOptions options,
        ContextLoader contextLoader)
    {
        _options = options;
        _contextLoader = contextLoader;
    }


    public override async Task<AnalysisTaskResult> Prepare(Models.Analysis model, params object[] args)
    {
        var stopwatch = new Stopwatch();

        stopwatch.Restart();

        var directoryPath = DirectoryManager.EnsureCreated(_options.DataPath, model.Key);

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
        var path = Path.Combine(_options.DataPath, key, "data.json");

        return await ArchiveManager.Archive(path, "gz");
    }

    public async override Task<Stream> Download(string key, params object[] args)
    {
        var path = Path.Combine(_options.DataPath, key, "data.h5ad");

        return await ArchiveManager.Archive(path, "gz");
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
