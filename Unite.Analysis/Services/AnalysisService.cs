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
        return DirectoryManager.EnsureCreated(_options.DataPath, key);
    }   
}
