using Unite.Orchestrator.Configuration.Options;
using Unite.Orchestrator.Docker;
using Unite.Orchestrator.Docker.Cache;

namespace Unite.Analysis.Web.Handlers;

public class DockerMonitoringHandler : Handler
{
    private readonly IOrchestratorOptions _options;
    private readonly DockerService _dockerService;

    public DockerMonitoringHandler(
        IOrchestratorOptions options,
        DockerService dockerService)
    {
        _options = options;
        _dockerService = dockerService;
    }


    public override void Prepare()
    {
    }

    public override void Handle()
    {
        var idleRecords = ContainerRecords.Records
            .Where(record => IsIdle(record.Value))
            .ToArray();

        foreach (var idleRecord in idleRecords)
        {
            _dockerService.Containers.Remove(idleRecord.Value.Id).Wait();

            ContainerRecords.Remove(idleRecord.Key);
        }
    }


    private bool IsIdle(ContainerRecord record)
    {
        return (DateTime.UtcNow - record.LastActive).TotalMinutes > _options.IdleTimeout;
    }
}
