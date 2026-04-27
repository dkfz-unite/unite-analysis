using Unite.Analysis.Web.Handlers;

namespace Unite.Analysis.Web.Workers;

public class AnalysisPreparingWorker : Worker<AnalysisPreparingHandler>
{
    public AnalysisPreparingWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<AnalysisPreparingWorker> logger) : base(scopeFactory, logger)
    {
    }
}
