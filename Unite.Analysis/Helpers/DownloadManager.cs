namespace Unite.Analysis.Helpers;

public static class DownloadManager
{
    public static async Task Download(string path, string url, string token)
    {
        // TODO: REMOVE
        // var uri = url.Replace("http://source.data.unite.net", "http://localhost:5400");

        using var client = CreateClient(token);
        using var responseTream = await client.GetStreamAsync(url);
        using var fileStream = File.OpenWrite(path);

        await responseTream.CopyToAsync(fileStream);
    }


    private static HttpClient CreateClient(string token)
    {
        var handler = new HttpClientHandler { UseProxy = false };
        var client = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(60), DefaultRequestHeaders = {{"Authorization", $"Bearer {token}"}} };

        return client;
    }
}
