using System.Text.Json;
using Unite.Analysis.Models.Enums;
using Unite.Analysis.Web.Configuration.Options;
using Unite.Analysis.Web.Handlers.Helpers;
using Unite.Analysis.Web.Services;
using Unite.Data.Entities.Tasks.Enums;

namespace Unite.Analysis.Web.Handlers;

public class AnalysisPreparingHandler
{
    private readonly ApiOptions _apiOptions;
    private readonly AnalysisTaskService _analysisTaskService;
    private readonly Analysis.Services.DonSce.AnalysisService _donSceAnalysisService;
    private readonly Analysis.Services.MethDm.AnalysisService _methDmAnalysisService;
    private readonly Analysis.Services.RnaDe.AnalysisService _rnaDeAnalysisService;
    private readonly Analysis.Services.RnascDc.AnalysisService _rnascDcAnalysisService;
    
    private readonly ILogger _logger;


    public AnalysisPreparingHandler(
        ApiOptions apiOptions,
        AnalysisTaskService analysisTaskService,
        Analysis.Services.DonSce.AnalysisService donSceAnalysisService,
        Analysis.Services.MethDm.AnalysisService methDmAnalysisService,
        Analysis.Services.RnaDe.AnalysisService rnaDeAnalysisService,
        Analysis.Services.RnascDc.AnalysisService rnascDcAnalysisService,
        ILogger<AnalysisPreparingHandler> logger)
    {
        _apiOptions = apiOptions;
        _analysisTaskService = analysisTaskService;
        _donSceAnalysisService = donSceAnalysisService;
        _methDmAnalysisService = methDmAnalysisService;
        _rnaDeAnalysisService = rnaDeAnalysisService;
        _rnascDcAnalysisService = rnascDcAnalysisService;
        
        _logger = logger;
    }


    public void Handle()
    {
       _analysisTaskService.Iterate(null, TaskStatusType.Preparing, TaskStatusType.Prepared, 10, 2000, task =>
       {
            return PrepareAnalysisTask(task);
       });
    }

    private async Task<byte> PrepareAnalysisTask(Data.Entities.Tasks.Task task)
    {
        var token = TokenHelper.Generate(_apiOptions.Key);

        var result = task.AnalysisTypeId switch
        {
            AnalysisTaskType.DON_SCE => await _donSceAnalysisService.Prepare(Parse<Analysis.Services.DonSce.Models.Criteria.Analysis>(task.Data), token),
            AnalysisTaskType.METH_DM => await _methDmAnalysisService.Prepare(Parse<Analysis.Services.MethDm.Models.Criteria.Analysis>(task.Data), token),
            AnalysisTaskType.RNA_DE => await _rnaDeAnalysisService.Prepare(Parse<Analysis.Services.RnaDe.Models.Criteria.Analysis>(task.Data), token),
            AnalysisTaskType.RNASC_DC => await _rnascDcAnalysisService.Prepare(Parse<Analysis.Services.RnascDc.Models.Criteria.Analysis>(task.Data), token),
            _ => throw new NotImplementedException($"Analysis task '{task.AnalysisTypeId}' is not supported")
        };

        if (result.Status == AnalysisTaskStatus.Success)
            _logger.LogInformation("Analysis task '{id}' ({type}) - prepared in {elapsed}s", task.Id, task.AnalysisTypeId, result.Elapsed);
        else if (result.Status == AnalysisTaskStatus.Failed)
            _logger.LogError("Analysis task '{id}' ({type}) - failed", task.Id, task.AnalysisTypeId);

        return (byte)result.Status;
    }

    private static T Parse<T>(string data) where T : class
    {
        return JsonSerializer.Deserialize<T>(data);
    }
}
