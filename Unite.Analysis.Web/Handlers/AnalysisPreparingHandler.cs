using System.Text.Json;
using Unite.Analysis.Expression;
using Unite.Analysis.Models;
using Unite.Analysis.Models.Enums;
using Unite.Analysis.Web.Services;
using Unite.Data.Entities.Tasks.Enums;

namespace Unite.Analysis.Web.Handlers;

public class AnalysisPreparingHandler
{
    private readonly AnalysisTaskService _analysisTaskService;
    private readonly ExpressionAnalysisService _expressionAnalysisService;
    private readonly ILogger _logger;


    public AnalysisPreparingHandler(
        AnalysisTaskService analysisTaskService,
        ExpressionAnalysisService expressionAnalysisService,
        ILogger<AnalysisPreparingHandler> logger)
    {
        _analysisTaskService = analysisTaskService;
        _expressionAnalysisService = expressionAnalysisService;
        _logger = logger;
    }


    public void Handle()
    {
       _analysisTaskService.Iterate(null, TaskStatusType.Preparing, TaskStatusType.Prepared, 10, 2000, task =>
       {
            return PrepareAnalysisTask(task);
       });
    }

    private async Task<byte> PrepareAnalysisTask(Unite.Data.Entities.Tasks.Task task)
    {
        var result = task.AnalysisTypeId switch
        {
            AnalysisTaskType.DExp => await PrepareDExpTask(task.Data),
            _ => throw new NotImplementedException()
        };

        if (result.Status == AnalysisTaskStatus.Success)
            _logger.LogInformation("Analysis task '{id}' ({type}) - prepared in {elapsed}s", task.Id, task.AnalysisTypeId, result.Elapsed);
        else if (result.Status == AnalysisTaskStatus.Failed)
            _logger.LogError("Analysis task '{id}' ({type}) - failed", task.Id, task.AnalysisTypeId);

        return (byte)result.Status;
    }

    private Task<AnalysisTaskResult> PrepareDExpTask(string data)
    {
        var model = JsonSerializer.Deserialize<Analysis.Expression.Models.Analysis>(data);

        return _expressionAnalysisService.Prepare(model);
    }
}
