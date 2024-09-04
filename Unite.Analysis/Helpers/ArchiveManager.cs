using System.IO.Compression;

namespace Unite.Analysis.Helpers;

public static class ArchiveManager
{
    public static async Task<Stream> Archive(string path, string archive)
    {
        if (archive == "zip")
            return await ArchiveZip(path);
        else if (archive == "gz")
            return await ArchiveGz(path);
        else
            throw new NotSupportedException($"Archive type '{archive}' is not supported");
    }

    public static async Task<Stream> ArchiveZip(string path)
    {
        var memoryStream = new MemoryStream();

        using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true);
        var entry = archive.CreateEntry(Path.GetFileName(path));

        using var entryStream = entry.Open();
        using var fileStream = File.OpenRead(path);

        await fileStream.CopyToAsync(entryStream);

        memoryStream.Position = 0;

        return memoryStream;
    }

    public static async Task<Stream> ArchiveGz(string path)
    {
        var memoryStream = new MemoryStream();

        using var archive = new GZipStream(memoryStream, CompressionLevel.Optimal, true);
        using var fileStream = File.OpenRead(path);

        await fileStream.CopyToAsync(archive);

        memoryStream.Position = 0;

        return memoryStream;
    }

    public static async Task Extract(string path, string archive)
    {
        if (archive == "zip")
            await ExtractZip(path);
        else if (archive == "gz")
            await ExtractGz(path);
        else
            throw new NotSupportedException($"Archive type '{archive}' is not supported");
    }


    private static async Task ExtractZip(string path)
    {
        ZipFile.ExtractToDirectory(path, Path.GetDirectoryName(path));

        File.Delete(path);

        await Task.CompletedTask;
    }

    private static async Task ExtractGz(string path)
    {
        using var sourceStream = File.OpenRead(path);
        using var archiveStream = new GZipStream(sourceStream, CompressionMode.Decompress);
        using var targetStream = File.OpenWrite(path.Replace(".gz", ""));

        await archiveStream.CopyToAsync(targetStream);

        File.Delete(path);
    }
}
