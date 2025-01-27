namespace Unite.Analysis.Helpers;

public static class DownloadManager
{
    public static async Task Download(string path, string url, string token, string host = null)
    {
        // Replace the host if it is not null, e.g.:
        // "http://source.data.unite.net" -> "http://localhost:5400"
        var uri = CreateUri(url, host);

        using var client = CreateClient(token);
        using var responseTream = await client.GetStreamAsync(uri);
        using var fileStream = File.OpenWrite(path);

        await responseTream.CopyToAsync(fileStream);
    }


    private static HttpClient CreateClient(string token)
    {
        var handler = new HttpClientHandler { UseProxy = false };
        var client = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(60), DefaultRequestHeaders = {{"Authorization", $"Bearer {token}"}} };

        return client;
    }

    private static Uri CreateUri(string url, string host)
    {
        var sourceUri = new UriBuilder(url);
        sourceUri.Scheme ??= "http";

        if (host != null)
        {
            var targetUri = new UriBuilder(host);
            targetUri.Scheme ??= "http";
            targetUri.Path = sourceUri.Path;
            targetUri.Query = sourceUri.Query;

            return targetUri.Uri;
        }
        else
        {
            return sourceUri.Uri;
        }
    }
}
