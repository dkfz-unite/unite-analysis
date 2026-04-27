using Unite.Analysis.Web.Handlers;

namespace Unite.Analysis.Web.Workers;

public class AnalysisProcessingWorker : Worker<AnalysisProcessingHandler>
{
    public AnalysisProcessingWorker(IServiceScopeFactory scopeFactory, ILogger<AnalysisProcessingWorker> logger) : base(scopeFactory, logger)
    {
    }
}
