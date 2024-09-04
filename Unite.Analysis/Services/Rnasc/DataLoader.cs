using Unite.Analysis.Helpers;
using Unite.Analysis.Services.Rnasc.Extensions;
using Unite.Data.Entities.Genome.Analysis;

namespace Unite.Analysis.Services.Rnasc;

public class DataLoader
{
    public static async Task DownloadResources(AnalysisContext context, string workingDirectoryPath, string token)
    {
        foreach (var sample in context.Samples)
        {
            await DownloadResource(sample.Value, sample.Value.GetKey(context), workingDirectoryPath, token);
        }
    }


    private static async Task DownloadResource(Sample sample, string key, string workingDirectoryPath, string token)
    {
        var resource = sample.Resources.FirstOrDefault(resource => resource.Type == "rnasc-exp");

        if (resource != null)
        {
            var sampleDirectoryName = key;
            var sampleDirectoryPath = DirectoryManager.EnsureCreated(workingDirectoryPath, sampleDirectoryName);

            var matrixFilePath = Path.Combine(sampleDirectoryPath, GetFileName("matrix", "mtx", resource.Archive));
            var featuresFilePath = Path.Combine(sampleDirectoryPath, GetFileName("features", "tsv", resource.Archive));
            var barcodesFilePath = Path.Combine(sampleDirectoryPath, GetFileName("barcodes", "tsv", resource.Archive));

            var matrixDownloadTask = DownloadManager.Download(matrixFilePath, $"{resource.Url}", token);
            var featuresDownloadTask = DownloadManager.Download(featuresFilePath, $"{resource.Url}/features", token);
            var barcodesDownloadTask = DownloadManager.Download(barcodesFilePath, $"{resource.Url}/barcodes", token);
            await Task.WhenAll(matrixDownloadTask, featuresDownloadTask, barcodesDownloadTask);
        }
        else
        {
            throw new Exception("Resource of type 'rnasc-exp' was not found");
        }
    }

    private static string GetFileName(string name, string format, string archive)
    {
        var namePart = name;
        var formatPart = format != null ? $".{format}" : "";
        var archivePart = archive != null ? $".{archive}" : "";

        return $"{namePart}{formatPart}{archivePart}";
    }
}
