namespace Unite.Analysis.Web.Services.Cache;

public class CacheRecord
{
    public string Name;
    public DateTime LastActive;
    public string LocalUrl;
    public string RemoteUrl;


    public CacheRecord()
    {
        
    }

    public CacheRecord(string name, DateTime lastActive, string localUrl, string remoteUrl)
    {
        Name = name;
        LastActive = lastActive;
        LocalUrl = localUrl;
        RemoteUrl = remoteUrl;
    } 
}
