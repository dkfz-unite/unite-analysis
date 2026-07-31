using System.Diagnostics;
using Unite.Analysis.Configuration.Options;
using Unite.Analysis.Helpers;
using Unite.Analysis.Models;

namespace Unite.Analysis.Services;

/// <summary>
/// Analysis service interface.
/// </summary>
/// <typeparam name="TModel">Analysis model type.</typeparam>
/// <typeparam name="TResult">Analysis result type.</typeparam>
public abstract class AnalysisService<TModel> where TModel : class
{
    protected readonly IAnalysisOptions _options;

    public static readonly string InputDirectoryName = "input";
    public static readonly string OutputDirectoryName = "output";
    public static readonly string OptionsFileName = Path.Combine(InputDirectoryName, "options.json");
    public static readonly string DataFileName = Path.Combine(InputDirectoryName, "data.tsv");
    public static readonly string MetadataFileName = Path.Combine(InputDirectoryName, "metadata.tsv");
    public static readonly string ArchiveFileName = "analysis.zip";



    public AnalysisService(IAnalysisOptions options)
    {
        _options = options;
    }


    /// <summary>
    /// Prepare required analysis data for further processing.
    /// </summary>
    /// <param name="model">Analysis model.</param>
    /// <returns>Analysis task status.</returns>
    public abstract Task<AnalysisTaskResult> Prepare(TModel model, params object[] args);

    /// <summary>
    /// Process prepared analysis data.
    /// </summary>
    /// <param name="key">Analysis task key.</param>
    /// <returns>Analysis task status.</returns> 
    public abstract Task<AnalysisTaskResult> Process(string key, params object[] args);

    /// <summary>
    /// Load analysis results metadata.
    /// </summary>
    /// <param name="key">Analysis task key.</param>
    /// <returns>Analysis results.</returns>
    public abstract Task<Stream> Load(string key, params object[] args);

    /// <summary>
    /// Download analysis results data.
    /// </summary>
    /// <param name="key">Analysis task key.</param>
    /// <returns>Analysis results.</returns>
    public abstract Task<Stream> Download(string key, params object[] args);

    /// <summary>
    /// Delete analysis task, it's data and results data.
    /// </summary>
    /// <param name="key">Analysis task key.</param>
    public abstract Task Delete(string key, params object[] args);


    public virtual async Task<AnalysisTaskResult> ProcessRemotely(string url)
    {
        var stopwatch = new Stopwatch();
        var httpClientHandler = new HttpClientHandler() { UseProxy = false };
        using var httpClient = new HttpClient(httpClientHandler) { Timeout = TimeSpan.FromMinutes(60) };

        stopwatch.Start();

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        var response = await httpClient.SendAsync(request);

        stopwatch.Stop();

        if (response.IsSuccessStatusCode)
        {
            return AnalysisTaskResult.Success(stopwatch.Elapsed.TotalSeconds);
        }
        else
        {
            var statusCode = (int)response.StatusCode;

            if (statusCode == 501)
                return AnalysisTaskResult.Rejected();
            else if (statusCode == 500)
                return AnalysisTaskResult.Failed();
            else 
                throw new NotImplementedException();
        }
    }

    protected string GetWorkingDirectoryPath(string key)
    {
        var path = DirectoryManager.EnsureCreated(_options.DataPath, key);
        DirectoryManager.EnsureCreated(path, InputDirectoryName);
        DirectoryManager.EnsureCreated(path, OutputDirectoryName);
        return path;
    }
}

public class GenericAnalysisService : AnalysisService<object>
{
    public GenericAnalysisService(IAnalysisOptions options) : base(options)
    {
    }

    public override Task<AnalysisTaskResult> Prepare(object model, params object[] args)
    {
        throw new NotImplementedException();
    }

    public override Task<AnalysisTaskResult> Process(string key, params object[] args)
    {
        throw new NotImplementedException();
    }

    public override Task<Stream> Load(string key, params object[] args)
    {
        throw new NotImplementedException();
    }

    public override Task<Stream> Download(string key, params object[] args)
    {
        throw new NotImplementedException();
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
