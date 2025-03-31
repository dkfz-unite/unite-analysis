using Unite.Analysis.Helpers;
using System.Text;
using Unite.Analysis.Services.Meth.Extensions;
using Unite.Analysis.Services.Meth.Models.Context;
using Unite.Data.Constants;
using Unite.Data.Entities.Genome.Analysis;

namespace Unite.Analysis.Services.Meth;

public class DataLoader
{
    public static async Task DownloadResources(AnalysisContext context, string workingDirectoryPath, string token, string datasetId, string dataPath = null, string host = null)
    {
        foreach (var sample in context.Samples)
        {
            await DownloadResource(sample.Value, sample.Value.GetKey(context), workingDirectoryPath, token, datasetId, dataPath, host);
        }
    }

    private static async Task DownloadResource(Sample sample, string key, string workingDirectoryPath, string token, string datasetId, string dataPath = null, string host = null)
    {
        var tsvData = string.Empty;
        Dictionary<string, string> idatFiles = new Dictionary<string, string>();
 
        var resources = sample.Resources.Where(resource => resource.Type == DataTypes.Genome.Meth.Sample).ToArray();
 
        var idatResourceFileName = resources.FirstOrDefault(resource =>
            resource.Format == FileTypes.Sequence.Idat).Name.Replace("_grn", "").Replace("_red", "");

        var sampleDirectoryName = key;
        var sampleDirectoryPath = DirectoryManager.EnsureCreated(workingDirectoryPath, sampleDirectoryName);

        tsvData = tsvData + key + '\t' + datasetId + '\t' + Path.Combine(sampleDirectoryPath, idatResourceFileName);
        
        PrepareSampleSheet(workingDirectoryPath, tsvData);

        foreach (var resource in resources)
        {
            if (resource == null)
                throw new Exception($"idat file is missing for `{key}`");

            var idatFilePath  =  Path.Combine(sampleDirectoryPath, $"{resource.Name}.{resource.Format}");
            var idatDownloadTask = DownloadManager.Download(idatFilePath, resource.Url, token, host);

            await Task.WhenAll(idatDownloadTask);
        }
    }

    private static void PrepareSampleSheet(string workingDirectoryPath, string data)
    {
        string sampleSheetFilePath =   Path.Combine(workingDirectoryPath, "sample_sheet.tsv");

        if (!File.Exists(sampleSheetFilePath))
        {
            using (var writer = new StreamWriter(sampleSheetFilePath, append: false, Encoding.UTF8))
            {
                writer.WriteLine("sample_id\tconditions\tpath");
            }
        }
        using (var writer = new StreamWriter(sampleSheetFilePath, append: true, Encoding.UTF8))
        {
            writer.WriteLine(data);
        }
    }
}
