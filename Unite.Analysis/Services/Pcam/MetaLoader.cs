using Unite.Data.Constants;
using Unite.Essentials.Tsv;

namespace Unite.Analysis.Services.Pcam;

public class MetaLoader
{
    public static async Task PrepareMetadata(SamplesContext context, string workingDirectoryPath)
    {
        var entries = Load(context, workingDirectoryPath);
        var path = Path.Combine(workingDirectoryPath, "metadata.tsv");
        var map = SampleMetadataMapper.Map(entries).Map(entry => entry.Path, "path");

        if (!File.Exists(path))
        {
            var tsv = TsvWriter.Write(entries, map);
            File.WriteAllText(path, tsv);
        }
        else
        {
            var tsv = TsvWriter.Write(entries, map, false);
            File.AppendAllText(path, tsv);
        }

        await Task.CompletedTask;
    }

    private static Models.Input.SampleMetadata[] Load(SamplesContext context, string workingDirectoryPath)
    {
        return SampleMetadataLoader.Load<Models.Input.SampleMetadata>(context, (sample, entry, context) =>
        {
            var resources = sample.Resources
                .Where(resource => resource.Type == DataTypes.Omics.Meth.Sample && resource.Format == FileTypes.Sequence.Idat)
                .ToArray();

            if (resources.Length == 0)
                return false;

            var fileName = resources.First().Name
                .Replace("_grn", "", StringComparison.InvariantCultureIgnoreCase)
                .Replace("grn", "", StringComparison.InvariantCultureIgnoreCase)
                .Replace("_red", "", StringComparison.InvariantCultureIgnoreCase)
                .Replace("red", "", StringComparison.InvariantCultureIgnoreCase)
                .Replace(".idat", "", StringComparison.InvariantCultureIgnoreCase);

            var filePath = Path.Combine(workingDirectoryPath, entry.Id, fileName);

            entry.Path = filePath;

            return true;
        });
    }
}
