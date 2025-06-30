using Unite.Analysis.Helpers;
using Unite.Data.Constants;
using Unite.Data.Entities.Omics.Analysis;

namespace Unite.Analysis.Services.Dm;

public class DataLoader
{
    public static async Task DownloadResources(SamplesContext context, string workingDirectoryPath, string token,string host = null)
    {
        foreach (var sample in context.OmicsSamples)
        {
            await DownloadResource(sample.Value, context.GetSampleKey(sample.Key), workingDirectoryPath, token,host);
        }
    }

    private static async Task DownloadResource(Sample sample, string key, string workingDirectoryPath, string token,string host = null)
    {
        var resources = sample.Resources.Where(resource => resource.Type == DataTypes.Omics.Meth.Sample).ToArray();
 
        var sampleDirectoryPath = DirectoryManager.EnsureCreated(workingDirectoryPath, key);

        foreach (var resource in resources)
        {
            if (resource == null)
                throw new Exception($"idat file is missing for `{key}`");

            var idatFilePath  =  Path.Combine(sampleDirectoryPath, $"{resource.Name}.{resource.Format}");
            var idatDownloadTask = DownloadManager.Download(idatFilePath, resource.Url, token, host);

            await Task.WhenAll(idatDownloadTask);
        }
    }
}
