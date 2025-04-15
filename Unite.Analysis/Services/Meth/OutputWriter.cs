using System.IO.Compression;

namespace Unite.Analysis.Services.Meth;

public class OutputWriter
{
    public const string ResultDataFileName = "results.csv.gz";
    public const string ReducedResultDataFileName = "results_reduced.csv.gz";
    public const string AnnotationDataFileName = "results_annotated.csv.gz";

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

        archive.CreateEntryFromFile(Path.Combine(path, ResultDataFileName), ResultDataFileName);
        archive.CreateEntryFromFile(Path.Combine(path, ReducedResultDataFileName), ReducedResultDataFileName);
        archive.CreateEntryFromFile(Path.Combine(path, AnnotationDataFileName), AnnotationDataFileName);

        await Task.CompletedTask;
    }
}
