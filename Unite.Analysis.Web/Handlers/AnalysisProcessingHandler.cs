using Unite.Analysis.Models.Enums;
using Unite.Analysis.Web.Configuration.Options;
using Unite.Analysis.Web.Handlers.Helpers;
using Unite.Analysis.Web.Services;
using Unite.Data.Entities.Tasks.Enums;

namespace Unite.Analysis.Web.Handlers;

public class AnalysisProcessingHandler
{    
    private readonly ApiOptions _apiOptions;
    private readonly AnalysisTaskService _analysisTaskService;
    private readonly Analysis.Services.RnaDe.AnalysisService _expressionAnalysisService;
    private readonly Analysis.Services.Rnasc.AnalysisService _scAnalysisService;
    private readonly ILogger _logger;


    public AnalysisProcessingHandler(
        ApiOptions apiOptions,
        AnalysisTaskService analysisTaskService,
        Analysis.Services.RnaDe.AnalysisService expressionAnalysisService,
        Analysis.Services.Rnasc.AnalysisService scAnalysisService,
        ILogger<AnalysisProcessingHandler> logger)
    {
        _apiOptions = apiOptions;
        _analysisTaskService = analysisTaskService;
        _expressionAnalysisService = expressionAnalysisService;
        _scAnalysisService = scAnalysisService;
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
        var token = TokenHelper.Generate(_apiOptions.Key);
        var result = task.AnalysisTypeId switch
        {
            AnalysisTaskType.RNA_DE => await _expressionAnalysisService.Process(task.Target, token),
            AnalysisTaskType.RNASC => await _scAnalysisService.Process(task.Target, token),
            _ => throw new NotImplementedException()
        };

        if (result.Status == AnalysisTaskStatus.Success)
            _logger.LogInformation("Analysis task '{id}' ({type}) - processed in {elapsed}s", task.Id, task.AnalysisTypeId, result.Elapsed);
        else if (result.Status == AnalysisTaskStatus.Failed)
            _logger.LogError("Analysis task '{id}' ({type}) - failed", task.Id, task.AnalysisTypeId);

        return (byte)result.Status;
    }
}
