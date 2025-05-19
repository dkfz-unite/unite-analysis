using Unite.Analysis.Helpers;
using Unite.Analysis.Services.Scell.Extensions;
using Unite.Analysis.Services.Scell.Models.Context;
using Unite.Data.Constants;
using Unite.Data.Entities.Omics.Analysis;

namespace Unite.Analysis.Services.Scell;

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
        var comparison = StringComparison.InvariantCultureIgnoreCase;

        var resources = sample.Resources.Where(resource => resource.Type == DataTypes.Omics.Rnasc.Exp).ToArray();

        var matrixResource = resources.FirstOrDefault(resource =>
            resource.Name.Equals("matrix", comparison) &&
            resource.Format == FileTypes.Sequence.Mtx && 
            resource.Archive == ArchiveTypes.Gz);

        if (matrixResource == null)
            throw new Exception($"matrix.tsv.gz file is missing for `{key}`");

        var featuresResource = resources.FirstOrDefault(resource =>
            resource.Name.Equals("features", comparison) &&
            resource.Format == FileTypes.General.Tsv && 
            resource.Archive == ArchiveTypes.Gz);

        if (featuresResource == null)
            throw new Exception($"features.tsv.gz file is missing for `{key}`");

        var barcodesResource = resources.FirstOrDefault(resource =>
            resource.Name.Equals("barcodes", comparison) &&
            resource.Format == FileTypes.General.Tsv && 
            resource.Archive == ArchiveTypes.Gz);
        
        if (barcodesResource == null)
            throw new Exception($"barcodes.tsv.gz file is missing for `{key}`");

        var sampleDirectoryPath = DirectoryManager.EnsureCreated(workingDirectoryPath, key);
        var matrixFilePath = Path.Combine(sampleDirectoryPath, $"{matrixResource.Name}.{matrixResource.Format}.{matrixResource.Archive}");
        var featuresFilePath = Path.Combine(sampleDirectoryPath, $"{featuresResource.Name}.{featuresResource.Format}.{featuresResource.Archive}");
        var barcodesFilePath = Path.Combine(sampleDirectoryPath, $"{barcodesResource.Name}.{barcodesResource.Format}.{barcodesResource.Archive}");

        var matrixDownloadTask = DownloadManager.Download(matrixFilePath, matrixResource.Url, token, host);
        var featuresDownloadTask = DownloadManager.Download(featuresFilePath, featuresResource.Url, token, host);
        var barcodesDownloadTask = DownloadManager.Download(barcodesFilePath, barcodesResource.Url, token, host);
        await Task.WhenAll(matrixDownloadTask, featuresDownloadTask, barcodesDownloadTask);
    }
}
