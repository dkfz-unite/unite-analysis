using Unite.Orchestrator.Configuration.Options;
using Unite.Orchestrator.Docker;
using Unite.Orchestrator.Docker.Cache;

namespace Unite.Analysis.Web.Handlers;

public class MonitoringHandler
{
    private readonly IOrchestratorOptions _options;
    private readonly DockerService _dockerService;
    private readonly ILogger _logger;


    public MonitoringHandler(
        IOrchestratorOptions options,
        DockerService dockerService,
        ILogger<MonitoringHandler> logger)
    {
        _options = options;
        _dockerService = dockerService;
        _logger = logger;
    }


    public async Task Handle()
    {
        var idleRecords = ContainerRecords.Records
            .Where(record => IsIdle(record.Value))
            .ToArray();

        foreach (var idleRecord in idleRecords)
        {
            await _dockerService.Containers.Remove(idleRecord.Value.Id);

            ContainerRecords.Remove(idleRecord.Key);
        }
    }


    private bool IsIdle(ContainerRecord record)
    {
        return (DateTime.UtcNow - record.LastActive).TotalMinutes > _options.IdleTimeout;
    }
}
