namespace Unite.Orchestrator.Docker.Cache;

public class ContainerRecord
{
    public string Id;
    public int Number;
    public DateTime LastActive;

    public ContainerRecord()
    {
        
    }

    public ContainerRecord(string id, int number, DateTime lastActive)
    {
        Id = id;
        Number = number;
        LastActive = lastActive;
    }
}
