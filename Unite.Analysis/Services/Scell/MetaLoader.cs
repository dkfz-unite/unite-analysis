using Unite.Essentials.Tsv;

namespace Unite.Analysis.Services.Scell;

public class MetaLoader
{
    private const string MetadataFileName = "metadata.tsv";

    public static async Task PrepareMetadata(SamplesContext context, string workingDirectoryPath)
    {
        var entries = SampleMetadataLoader.Load(context);

        var map = SampleMetadataMapper.Map(entries);

        var tsv = TsvWriter.Write(entries, map);

        File.WriteAllText(Path.Combine(workingDirectoryPath, MetadataFileName), tsv);

        await Task.CompletedTask;
    }
}
