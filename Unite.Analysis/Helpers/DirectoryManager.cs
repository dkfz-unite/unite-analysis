namespace Unite.Analysis.Helpers;

public static class DirectoryManager
{
    public static string EnsureCreated(string path, string name)
    {
        var directoryPath = Path.Combine(path, name);

        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, true);
        }

        Directory.CreateDirectory(directoryPath);

        return directoryPath;
    }
}
