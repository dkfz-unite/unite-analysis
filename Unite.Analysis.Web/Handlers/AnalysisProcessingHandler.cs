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
    private readonly Analysis.Services.DonSce.AnalysisService _donSceAnalysisService;
    private readonly Analysis.Services.MethDm.AnalysisService _methDmAnalysisService;
    private readonly Analysis.Services.RnaDe.AnalysisService _rnaDeAnalysisService;
    private readonly Analysis.Services.RnascDc.AnalysisService _rnascDcAnalysisService;
    private readonly ILogger _logger;


    public AnalysisProcessingHandler(
        ApiOptions apiOptions,
        AnalysisTaskService analysisTaskService,
        Analysis.Services.DonSce.AnalysisService donSceAnalysisService,
        Analysis.Services.MethDm.AnalysisService methDmAnalysisService,
        Analysis.Services.RnaDe.AnalysisService rnaDeAnalysisService,
        Analysis.Services.RnascDc.AnalysisService rnascDcAnalysisService,
        ILogger<AnalysisProcessingHandler> logger)
    {
        _apiOptions = apiOptions;
        _donSceAnalysisService = donSceAnalysisService;
        _methDmAnalysisService = methDmAnalysisService;
        _analysisTaskService = analysisTaskService;
        _rnaDeAnalysisService = rnaDeAnalysisService;
        _rnascDcAnalysisService = rnascDcAnalysisService;
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
            AnalysisTaskType.DON_SCE => await _donSceAnalysisService.Process(task.Target, token),
            AnalysisTaskType.METH_DM => await _methDmAnalysisService.Process(task.Target, token),
            AnalysisTaskType.RNA_DE => await _rnaDeAnalysisService.Process(task.Target, token),
            AnalysisTaskType.RNASC_DC => await _rnascDcAnalysisService.Process(task.Target, token),
            _ => throw new NotImplementedException($"Analysis task '{task.AnalysisTypeId}' is not supported")
        };

        if (result.Status == AnalysisTaskStatus.Success)
            _logger.LogInformation("Analysis task '{id}' ({type}) - processed in {elapsed}s", task.Id, task.AnalysisTypeId, result.Elapsed);
        else if (result.Status == AnalysisTaskStatus.Failed)
            _logger.LogError("Analysis task '{id}' ({type}) - failed", task.Id, task.AnalysisTypeId);

        return (byte)result.Status;
    }
}
