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
    private readonly Analysis.Services.Surv.AnalysisService _survSceAnalysisService;
    private readonly Analysis.Services.Dm.AnalysisService _dmAnalysisService;
    private readonly Analysis.Services.De.AnalysisService _deAnalysisService;
    private readonly Analysis.Services.Scell.AnalysisService _scellAnalysisService;
    private readonly ILogger _logger;


    public AnalysisProcessingHandler(
        ApiOptions apiOptions,
        AnalysisTaskService analysisTaskService,
        Analysis.Services.Surv.AnalysisService survAnalysisService,
        Analysis.Services.Dm.AnalysisService dmAnalysisService,
        Analysis.Services.De.AnalysisService deAnalysisService,
        Analysis.Services.Scell.AnalysisService scellAnalysisService,
        ILogger<AnalysisProcessingHandler> logger)
    {
        _apiOptions = apiOptions;
        _analysisTaskService = analysisTaskService;
        _survSceAnalysisService = survAnalysisService;
        _dmAnalysisService = dmAnalysisService;
        _deAnalysisService = deAnalysisService;
        _scellAnalysisService = scellAnalysisService;
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
            AnalysisTaskType.SURV => await _survSceAnalysisService.Process(task.Target, token),
            AnalysisTaskType.DM => await _dmAnalysisService.Process(task.Target, token),
            AnalysisTaskType.DE => await _deAnalysisService.Process(task.Target, token),
            AnalysisTaskType.SCELL => await _scellAnalysisService.Process(task.Target, token),
            _ => throw new NotImplementedException($"Analysis task '{task.AnalysisTypeId}' is not supported")
        };

        if (result.Status == AnalysisTaskStatus.Success)
            _logger.LogInformation("Analysis task '{id}' ({type}) - processed in {elapsed}s", task.Id, task.AnalysisTypeId, result.Elapsed);
        else if (result.Status == AnalysisTaskStatus.Failed)
            _logger.LogError("Analysis task '{id}' ({type}) - failed", task.Id, task.AnalysisTypeId);

        return (byte)result.Status;
    }
}
