using Unite.Analysis.Helpers;
using Unite.Analysis.Services.SCell.Extensions;
using Unite.Analysis.Services.SCell.Models.Context;
using Unite.Data.Entities.Genome.Analysis;

namespace Unite.Analysis.Services.SCell;

public class DataLoader
{
    public static async Task DownloadResources(AnalysisContext context, string workingDirectoryPath, string token, string host = null)
    {
        foreach (var sample in context.Samples)
        {
            await DownloadResource(sample.Value, sample.Value.GetKey(context), workingDirectoryPath, token, host);
        }
    }


    private static async Task DownloadResource(Sample sample, string key, string workingDirectoryPath, string token, string host = null)
    {
        var resource = sample.Resources.FirstOrDefault(resource => resource.Type == "rnasc-exp");

        if (resource != null)
        {
            var sampleDirectoryName = key;
            var sampleDirectoryPath = DirectoryManager.EnsureCreated(workingDirectoryPath, sampleDirectoryName);

            var matrixFilePath = Path.Combine(sampleDirectoryPath, GetFileName("matrix", "mtx", resource.Archive));
            var featuresFilePath = Path.Combine(sampleDirectoryPath, GetFileName("features", "tsv", resource.Archive));
            var barcodesFilePath = Path.Combine(sampleDirectoryPath, GetFileName("barcodes", "tsv", resource.Archive));

            var matrixDownloadTask = DownloadManager.Download(matrixFilePath, $"{resource.Url}", token, host);
            var featuresDownloadTask = DownloadManager.Download(featuresFilePath, $"{resource.Url}/features", token, host);
            var barcodesDownloadTask = DownloadManager.Download(barcodesFilePath, $"{resource.Url}/barcodes", token, host);
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
