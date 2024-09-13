using System.IO.Compression;

namespace Unite.Analysis.Helpers;

public static class ArchiveManager
{
    public static async Task<byte[]> Archive(string path, string archive)
    {
        if (archive == "zip")
            return await ArchiveZip(path);
        else if (archive == "gz")
            return await ArchiveGz(path);
        else
            throw new NotSupportedException($"Archive type '{archive}' is not supported");
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


    private static async Task<byte[]> ArchiveZip(string path)
    {
        using var stream = new MemoryStream();
        using var archive = new ZipArchive(stream, ZipArchiveMode.Create, true);
        var entry = archive.CreateEntry(Path.GetFileName(path));

        using var entryStream = entry.Open();
        using var fileStream = File.OpenRead(path);

        fileStream.CopyTo(entryStream);

        stream.Seek(0, SeekOrigin.Begin);

        return await Task.FromResult(stream.ToArray());
    }

    private static async Task<byte[]> ArchiveGz(string path)
    {
        using var stream = new MemoryStream();
        using var archive = new GZipStream(stream, CompressionLevel.Optimal, true);
        using var fileStream = File.OpenRead(path);

        fileStream.CopyTo(archive);

        stream.Seek(0, SeekOrigin.Begin);

        return await Task.FromResult(stream.ToArray());
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
