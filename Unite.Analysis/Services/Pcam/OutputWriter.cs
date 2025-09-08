using System.IO.Compression;

namespace Unite.Analysis.Services.Pcam;

public class OutputWriter
{
    public const string ResultsFileName = "results.tsv";
    public const string MetadataFileName = "metadata.tsv";
    public const string OptionsFileName = "options.json";
    public const string ArchiveFileName = "output.zip";

    public static async Task ProcessOutput(string path)
    {
        if (Directory.Exists(path))
        {
            foreach (var subPath in Directory.GetDirectories(path))
            {
                Directory.Delete(subPath, true);
            }
        }

        await Task.CompletedTask;
    }

    public static async Task ArchiveOutput(string path)
    {
        using var archiveStream = new FileStream(Path.Combine(path, ArchiveFileName), FileMode.CreateNew);
        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, false);

        archive.CreateEntryFromFile(Path.Combine(path, MetadataFileName), MetadataFileName);
        archive.CreateEntryFromFile(Path.Combine(path, ResultsFileName), ResultsFileName);
        archive.CreateEntryFromFile(Path.Combine(path, OptionsFileName), OptionsFileName);

        await Task.CompletedTask;
    }
}
