using Unite.Analysis.Helpers;
using Unite.Data.Constants;
using Unite.Analysis.Services.Dm.Models.Data;
using Unite.Essentials.Extensions;
using Unite.Essentials.Tsv;

namespace Unite.Analysis.Services.Dm;

public class MetaLoader
{
    public static MetadataSample[] Load(SamplesContext context, string workingDirectoryPath, string datasetId)
    {
        var metadataSamples = new List<MetadataSample>();
        
        foreach (var omicsSample in context.OmicsSamples)
        {
            var key = context.GetSampleKey(omicsSample.Key);

            var resources = omicsSample.Value.Resources
                .Where(resource => resource.Type == DataTypes.Omics.Meth.Sample && resource.Format == FileTypes.Sequence.Idat)
                .ToArray();

            if (resources.Length == 0)
                continue;

            var fileName = resources.FirstOrDefault().Name
                .Replace("_grn", "", StringComparison.InvariantCultureIgnoreCase)
                .Replace("_red", "", StringComparison.InvariantCultureIgnoreCase);

            var sampleDirectoryPath = DirectoryManager.EnsureCreated(workingDirectoryPath, key);

            var metadataSample = new MetadataSample
            {
                SampleId = key,
                Condition = datasetId,
                Path = Path.Combine(sampleDirectoryPath, fileName)
            };

            metadataSamples.Add(metadataSample);
        }

        return metadataSamples.ToArrayOrNull();
    }

    public static async Task PrepareMetadata(SamplesContext context, string workingDirectoryPath, string datasetId)
    {
        var entries = Load(context, workingDirectoryPath, datasetId);
        var metaFilePath = Path.Combine(workingDirectoryPath, "metadata.tsv");

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
