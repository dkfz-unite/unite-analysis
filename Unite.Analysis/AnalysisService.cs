using System.Diagnostics;
using Unite.Analysis.Models;

namespace Unite.Analysis;

public abstract class AnalysisService<TModel, TResult> where TModel : class
{
    public abstract Task<AnalysisTaskResult> Prepare(TModel model);
    public abstract Task<AnalysisTaskResult> Process(string key);
    public abstract Task<TResult> LoadResult(string key);
    public abstract Task<TResult> DownloadResult(string key);
    public abstract Task DeleteData(string key);

    public virtual async Task<AnalysisTaskResult> ProcessRemotely(string url)
    {
        var stopwatch = new Stopwatch();
        var httpClientHandler = new HttpClientHandler() { UseProxy = false };
        var httpClient = new HttpClient(httpClientHandler) { Timeout = TimeSpan.FromMinutes(60) };

        try
        {
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
        finally
        {
            stopwatch.Stop();
            httpClient.Dispose();
        }
    }
}
