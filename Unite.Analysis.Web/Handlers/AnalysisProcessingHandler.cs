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
    private readonly Analysis.Services.Pcam.AnalysisService _pcamAnalysisService;
    private readonly Analysis.Services.Deg.AnalysisService _degAnalysisService;
    private readonly Analysis.Services.Gaf.AnalysisService _gafAnalysisService;
    private readonly Analysis.Services.Dep.AnalysisService _depAnalysisService;
    private readonly Analysis.Services.Umapp.AnalysisService _umappAnalysisService;
    private readonly Analysis.Services.Scell.AnalysisService _scellAnalysisService;
    private readonly ILogger _logger;


    public AnalysisProcessingHandler(
        ApiOptions apiOptions,
        AnalysisTaskService analysisTaskService,
        Analysis.Services.Surv.AnalysisService survAnalysisService,
        Analysis.Services.Dm.AnalysisService dmAnalysisService,
        Analysis.Services.Pcam.AnalysisService pcamAnalysisService,
        Analysis.Services.Deg.AnalysisService degAnalysisService,
        Analysis.Services.Gaf.AnalysisService gafAnalysisService,
        Analysis.Services.Dep.AnalysisService depAnalysisService,
        Analysis.Services.Umapp.AnalysisService umappAnalysisService,
        Analysis.Services.Scell.AnalysisService scellAnalysisService,
        ILogger<AnalysisProcessingHandler> logger)
    {
        _apiOptions = apiOptions;
        _analysisTaskService = analysisTaskService;
        _survSceAnalysisService = survAnalysisService;
        _dmAnalysisService = dmAnalysisService;
        _pcamAnalysisService = pcamAnalysisService;
        _degAnalysisService = degAnalysisService;
        _gafAnalysisService = gafAnalysisService;
        _depAnalysisService = depAnalysisService;
        _umappAnalysisService = umappAnalysisService;
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


    private async Task<byte> ProcessAnalysisTask(Data.Entities.Tasks.Task task)
    {
        var token = TokenHelper.Generate(_apiOptions.Key);

        var result = task.AnalysisTypeId switch
        {
            AnalysisTaskType.SURV => await _survSceAnalysisService.Process(task.Target, token),
            AnalysisTaskType.DM => await _dmAnalysisService.Process(task.Target, token),
            AnalysisTaskType.PCAM => await _pcamAnalysisService.Process(task.Target, token),
            AnalysisTaskType.DEG => await _degAnalysisService.Process(task.Target, token),
            AnalysisTaskType.GAF => await _gafAnalysisService.Process(task.Target, token),
            AnalysisTaskType.DEP => await _depAnalysisService.Process(task.Target, token),
            AnalysisTaskType.UMAPP => await _umappAnalysisService.Process(task.Target, token),
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
