namespace Unite.Analysis.Web.Services.Cache;

public class CacheRecord
{
    public string Name;
    public int Number;
    public DateTime LastActive;

    public CacheRecord()
    {
        
    }

    public CacheRecord(string name, int number, DateTime lastActive)
    {
        Name = name;
        Number = number;
        LastActive = lastActive;
    }
}
