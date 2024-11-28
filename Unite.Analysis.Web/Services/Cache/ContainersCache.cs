namespace Unite.Analysis.Web.Services.Cache;

public static class ContainersCache
{
    private static readonly Dictionary<string, CacheRecord> _records = [];

    public static IEnumerable<KeyValuePair<string, CacheRecord>> Records => _records.AsEnumerable();


    public static bool TryGet(string key, out CacheRecord record)
    {
        return _records.TryGetValue(key, out record);
    }

    public static void Add(string key, CacheRecord record)
    {
        _records.TryAdd(key, record);
    }

    public static void Remove(string key)
    {
        _records.Remove(key);
    }
}
