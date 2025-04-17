using Unite.Analysis.Helpers;
using Unite.Analysis.Services.Dm.Extensions;
using Unite.Analysis.Services.Dm.Models.Context;
using Unite.Data.Constants;
using Unite.Data.Entities.Genome.Analysis;
using Unite.Analysis.Services.Dm.Models.Data;
using Unite.Essentials.Extensions;
using Unite.Essentials.Tsv;

namespace Unite.Analysis.Services.Dm;

public class DataLoader
{
    public static async Task DownloadResources(AnalysisContext context, string workingDirectoryPath, string token,string host = null)
    {
        foreach (var sample in context.Samples)
        {
            await DownloadResource(sample.Value, sample.Value.GetKey(context), workingDirectoryPath, token,host);
        }
    }

    private static async Task DownloadResource(Sample sample, string key, string workingDirectoryPath, string token,string host = null)
    {
        var resources = sample.Resources.Where(resource => resource.Type == DataTypes.Genome.Meth.Sample).ToArray();
 
        var idatResourceFileName = resources.FirstOrDefault(resource =>
            resource.Format == FileTypes.Sequence.Idat).Name.Replace("_grn", "").Replace("_red", "");

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

    public static Metadata[] Load(AnalysisContext context, string workingDirectoryPath, string datasetId)
    {
        List<Metadata> metadataList = new List<Metadata>();
        foreach (var sample in context.Samples)
        {
            string key = sample.Value.GetKey(context);
            var resources = sample.Value.Resources.Where(resource => resource.Type == DataTypes.Genome.Meth.Sample).ToArray();
    
            var idatResourceFileName = resources.FirstOrDefault(resource =>
                resource.Format == FileTypes.Sequence.Idat).Name.Replace("_grn", "").Replace("_red", "");

            var sampleDirectoryPath = DirectoryManager.EnsureCreated(workingDirectoryPath, key);

            var metadata = new Metadata();
            metadata.SampleId = key;
            metadata.Conditions = datasetId;
            metadata.Path = Path.Combine(sampleDirectoryPath, idatResourceFileName);

            metadataList.Add(metadata);
        }
        return metadataList.ToArrayOrNull();
    }

    public static async Task PrepareMetadata(AnalysisContext context, string workingDirectoryPath, string datasetId)
    {
        var entries = Load(context, workingDirectoryPath, datasetId);
        string metaFilePath = Path.Combine(workingDirectoryPath, "metadata.tsv");

        var map = MetaMapper.Map(entries);

        if (!File.Exists(metaFilePath))
        {
            var tsv = TsvWriter.Write(entries, map);
            File.WriteAllText(metaFilePath, tsv); 
        }
        else
        {
            var tsv = TsvWriter.Write(entries, map, false);
            File.AppendAllText(metaFilePath, tsv);
        }
        await Task.CompletedTask;
    }
}
