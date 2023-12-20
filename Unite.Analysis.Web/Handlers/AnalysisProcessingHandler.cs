using Unite.Analysis.Expression;
using Unite.Analysis.Models.Enums;
using Unite.Analysis.Web.Services;
using Unite.Data.Entities.Tasks.Enums;

namespace Unite.Analysis.Web.Handlers;

public class AnalysisProcessingHandler
{    
    private readonly AnalysisTaskService _analysisTaskService;
    private readonly ExpressionAnalysisService _expressionAnalysisService;
    private readonly ILogger _logger;


    public AnalysisProcessingHandler(
        AnalysisTaskService analysisTaskService,
        ExpressionAnalysisService expressionAnalysisService,
        ILogger<AnalysisProcessingHandler> logger)
    {
        _analysisTaskService = analysisTaskService;
        _expressionAnalysisService = expressionAnalysisService;
        _logger = logger;
    }


    public void Handle()
    {
        _analysisTaskService.Iterate(TaskStatusType.Prepared, TaskStatusType.Processing, TaskStatusType.Processed, 10, 2000, task => 
        {
            return ProcessAnalysisTask(task);
        });
    }


    private async Task<byte> ProcessAnalysisTask(Unite.Data.Entities.Tasks.Task task)
    {
        var result = task.AnalysisTypeId switch
        {
            AnalysisTaskType.DExp => await _expressionAnalysisService.Process(task.Target),
            _ => throw new NotImplementedException()
        };

        if (result.Status == AnalysisTaskStatus.Success)
            _logger.LogInformation("Analysis task '{id}' ({type}) - processed in {elapsed}s", task.Id, task.AnalysisTypeId, result.Elapsed);
        else if (result.Status == AnalysisTaskStatus.Failed)
            _logger.LogError("Analysis task '{id}' ({type}) - failed", task.Id, task.AnalysisTypeId);

        return (byte)result.Status;
    }
}
