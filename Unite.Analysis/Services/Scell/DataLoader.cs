using Unite.Analysis.Helpers;
using Unite.Data.Constants;
using Unite.Data.Entities.Omics.Analysis;

namespace Unite.Analysis.Services.Scell;

public class DataLoader
{   
    private const string BarcodesFileName = "barcodes.tsv.gz";
    private const string FeaturesFileName = "features.tsv.gz";
    private const string MatrixFileName = "matrix.mtx.gz";


    public static async Task DownloadResources(SamplesContext context, string workingDirectoryPath, string token, string host = null)
    {
        var wrongSampleIds = new List<int>();

        foreach (var sample in context.OmicsSamples)
        {
            try
            {
                await DownloadResource(sample.Value, context.GetSampleKey(sample.Key), workingDirectoryPath, token, host);
            }
            catch
            {
                wrongSampleIds.Add(sample.Key);
            }
        }

        context.RemoveSample(wrongSampleIds.ToArray());
    }

    private static async Task DownloadResource(Sample sample, string key, string workingDirectoryPath, string token, string host = null)
    {
        var comparison = StringComparison.InvariantCultureIgnoreCase;

        var resources = sample.Resources.Where(resource => resource.Type == DataTypes.Omics.Rnasc.Exp).ToArray();

        var barcodesResource = resources.FirstOrDefault(resource =>
            resource.Name.Equals(BarcodesFileName, comparison) &&
            resource.Format == FileTypes.General.Tsv && 
            resource.Archive == ArchiveTypes.Gz);
        
        if (barcodesResource == null)
            throw new Exception($"{BarcodesFileName} file is missing for `{key}`");

        var featuresResource = resources.FirstOrDefault(resource =>
            resource.Name.Equals(FeaturesFileName, comparison) &&
            resource.Format == FileTypes.General.Tsv && 
            resource.Archive == ArchiveTypes.Gz);

        if (featuresResource == null)
            throw new Exception($"{FeaturesFileName} file is missing for `{key}`");

        var matrixResource = resources.FirstOrDefault(resource =>
            resource.Name.Equals(MatrixFileName, comparison) &&
            resource.Format == FileTypes.Sequence.Mtx && 
            resource.Archive == ArchiveTypes.Gz);

        if (matrixResource == null)
            throw new Exception($"{MatrixFileName} file is missing for `{key}`");

        var sampleDirectoryPath = DirectoryManager.EnsureCreated(workingDirectoryPath, key);
        var barcodesFilePath = Path.Combine(sampleDirectoryPath, barcodesResource.Name);
        var featuresFilePath = Path.Combine(sampleDirectoryPath, featuresResource.Name);
        var matrixFilePath = Path.Combine(sampleDirectoryPath, matrixResource.Name);
        
        var barcodesDownloadTask = DownloadManager.Download(barcodesFilePath, barcodesResource.Url, token, host);
        var featuresDownloadTask = DownloadManager.Download(featuresFilePath, featuresResource.Url, token, host);
        var matrixDownloadTask = DownloadManager.Download(matrixFilePath, matrixResource.Url, token, host);

        await Task.WhenAll(barcodesDownloadTask, featuresDownloadTask, matrixDownloadTask);
    }
}
