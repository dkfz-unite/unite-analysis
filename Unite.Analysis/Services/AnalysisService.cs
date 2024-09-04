using System.Diagnostics;
using Unite.Analysis.Models;

namespace Unite.Analysis;

/// <summary>
/// Analysis service interface.
/// </summary>
/// <typeparam name="TModel">Analysis model type.</typeparam>
/// <typeparam name="TResult">Analysis result type.</typeparam>
public abstract class AnalysisService<TModel, TResult> where TModel : class
{
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
    /// Load analysis result.
    /// </summary>
    /// <param name="key">Analysis task key.</param>
    /// <returns>Analysis results.</returns>
    public abstract Task<TResult> Load(string key, params object[] args);

    /// <summary>
    /// Download analysis result.
    /// </summary>
    /// <param name="key">Analysis task key.</param>
    /// <returns>Analysis results.</returns>
    public abstract Task<TResult> Download(string key, params object[] args);

    /// <summary>
    /// Delete analysis task, it's data and result.
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
}
