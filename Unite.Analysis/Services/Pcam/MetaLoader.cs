using Unite.Analysis.Helpers;
using Unite.Analysis.Services.Pcam.Models.Input;
using Unite.Data.Constants;
using Unite.Essentials.Extensions;
using Unite.Essentials.Tsv;

namespace Unite.Analysis.Services.Pcam;

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

            var fileName = resources.First().Name
                .Replace("_grn", "", StringComparison.InvariantCultureIgnoreCase)
                .Replace("grn", "", StringComparison.InvariantCultureIgnoreCase)
                .Replace("_red", "", StringComparison.InvariantCultureIgnoreCase)
                .Replace("red", "", StringComparison.InvariantCultureIgnoreCase);

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
        var filePath = Path.Combine(workingDirectoryPath, "metadata.tsv");
        var map = new ClassMap<MetadataSample>().AutoMap();

        if (!File.Exists(filePath))
        {
            var tsv = TsvWriter.Write(entries, map);
            File.WriteAllText(filePath, tsv);
        }
        else
        {
            var tsv = TsvWriter.Write(entries, map, false);
            File.AppendAllText(filePath, tsv);
        }

        await Task.CompletedTask;
    }
}
