namespace Unite.Analysis.Helpers;

public static class DirectoryManager
{
    public static string EnsureCreated(string path, string name)
    {
        var directoryPath = Path.Combine(path, name);

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        return directoryPath;
    }

    public static void Delete(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }

    public static void DeleteSubdirectories(string path)
    {
        if (Directory.Exists(path))
        {
            foreach(var directoryPath in Directory.GetDirectories(path))
            {
                Directory.Delete(directoryPath, true);
            }
        }
    }
}
