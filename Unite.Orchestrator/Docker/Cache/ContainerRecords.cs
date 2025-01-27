namespace Unite.Orchestrator.Docker.Cache;

public class ContainerRecords
{
    private static readonly Dictionary<string, ContainerRecord> _records = [];

    public static IEnumerable<KeyValuePair<string, ContainerRecord>> Records => _records.AsEnumerable();


    public static bool TryGet(string key, out ContainerRecord record)
    {
        return _records.TryGetValue(key, out record);
    }

    public static void Add(string key, ContainerRecord record)
    {
        _records.TryAdd(key, record);
    }

    public static void Remove(string key)
    {
        _records.Remove(key);
    }
}
