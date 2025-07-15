using Unite.Analysis.Helpers;
using Unite.Data.Constants;
using Unite.Data.Entities.Omics.Analysis;

namespace Unite.Analysis.Services.Dm;

public class DataLoader
{
    public static async Task DownloadResources(SamplesContext context, string workingDirectoryPath, string token, string host = null)
    {
        var wrongSampleIds = new List<int>();

        foreach (var sample in context.OmicsSamples)
        {
            try
            {
                await DownloadResources(sample.Value, context.GetSampleKey(sample.Key), workingDirectoryPath, token, host);
            }
            catch
            {
                wrongSampleIds.Add(sample.Key);
            }
        }

        context.RemoveSample(wrongSampleIds.ToArray());
    }

    private static async Task DownloadResources(Sample sample, string key, string workingDirectoryPath, string token,string host = null)
    {
        var resources = sample.Resources
            .Where(resource => resource.Type == DataTypes.Omics.Meth.Sample && resource.Format == FileTypes.Sequence.Idat)
            .ToArray();

        var redResource = resources.FirstOrDefault(resource => resource.Name.Contains("red", StringComparison.InvariantCultureIgnoreCase));
        if (redResource == null)
            throw new Exception($"Red channel IDAT file is missing for `{key}`");

        var grnResource = resources.FirstOrDefault(resource => resource.Name.Contains("grn", StringComparison.InvariantCultureIgnoreCase));
        if (grnResource == null)
            throw new Exception($"Green channel IDAT file is missing for `{key}`");

        var sampleDirectoryPath = DirectoryManager.EnsureCreated(workingDirectoryPath, key);
        
        var redFilePath = Path.Combine(sampleDirectoryPath, redResource.Name);
        var grnFilePath = Path.Combine(sampleDirectoryPath, grnResource.Name);

        var redDownloadTask = DownloadManager.Download(redFilePath, redResource.Url, token, host);
        var grnDownloadTask = DownloadManager.Download(grnFilePath, grnResource.Url, token, host);

        await Task.WhenAll(redDownloadTask, grnDownloadTask);
    }
}
