using Unite.Analysis.Web.Handlers;

namespace Unite.Analysis.Web.Workers;

public class DockerMonitoringWorker : Worker<DockerMonitoringHandler>
{
    public DockerMonitoringWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<DockerMonitoringWorker> logger) : base(scopeFactory, logger)
    {
    }
}
