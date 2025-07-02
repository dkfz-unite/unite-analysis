using Unite.Analysis.Helpers;
using Unite.Data.Constants;
using Unite.Analysis.Services.Pcam.Models.Data;
using Unite.Essentials.Extensions;
using Unite.Essentials.Tsv;
using Unite.Data.Entities.Omics.Analysis;

namespace Unite.Analysis.Services.Pcam;

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

    public static Metadata[] Load(SamplesContext context, string workingDirectoryPath, string datasetId)
    {
        List<Metadata> metadataList = new List<Metadata>();
        foreach (var sample in context.OmicsSamples)
        {
            string key = context.GetSampleKey(sample.Key);
            var resources = sample.Value.Resources.Where(resource => resource.Type == DataTypes.Omics.Meth.Sample).ToArray();
    
            var idatResourceFileName = resources.FirstOrDefault(resource =>
            resource.Format == FileTypes.Sequence.Idat).Name.Replace("_grn", "", StringComparison.InvariantCultureIgnoreCase).Replace("_red", "", StringComparison.InvariantCultureIgnoreCase);

            var sampleDirectoryPath = DirectoryManager.EnsureCreated(workingDirectoryPath, key);

            var metadata = new Metadata();
            metadata.SampleId = key;
            metadata.Conditions = datasetId;
            metadata.Path = Path.Combine(sampleDirectoryPath, idatResourceFileName);

            metadataList.Add(metadata);
        }
        return metadataList.ToArrayOrNull();
    }

    public static async Task PrepareMetadata(SamplesContext context, string workingDirectoryPath, string datasetId)
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