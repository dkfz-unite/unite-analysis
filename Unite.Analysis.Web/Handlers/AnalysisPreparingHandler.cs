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
    private readonly Analysis.Services.Surv.AnalysisService _survSceAnalysisService;
    private readonly Analysis.Services.Dm.AnalysisService _dmAnalysisService;
    private readonly Analysis.Services.Pcam.AnalysisService _pcamAnalysisService;
    private readonly Analysis.Services.De.AnalysisService _deAnalysisService;
    private readonly Analysis.Services.Gaf.AnalysisService _gafAnalysisService;
    private readonly Analysis.Services.Scell.AnalysisService _scellDcAnalysisService;
    
    private readonly ILogger _logger;


    public AnalysisPreparingHandler(
        ApiOptions apiOptions,
        AnalysisTaskService analysisTaskService,
        Analysis.Services.Surv.AnalysisService survAnalysisService,
        Analysis.Services.Dm.AnalysisService dmAnalysisService,
        Analysis.Services.Pcam.AnalysisService pcamAnalysisService,
        Analysis.Services.De.AnalysisService deAnalysisService,
        Analysis.Services.Gaf.AnalysisService gafAnalysisService,
        Analysis.Services.Scell.AnalysisService scellDcAnalysisService,
        ILogger<AnalysisPreparingHandler> logger)
    {
        _apiOptions = apiOptions;
        _analysisTaskService = analysisTaskService;
        _survSceAnalysisService = survAnalysisService;
        _dmAnalysisService = dmAnalysisService;
        _pcamAnalysisService = pcamAnalysisService;
        _deAnalysisService = deAnalysisService;
        _gafAnalysisService = gafAnalysisService;
        _scellDcAnalysisService = scellDcAnalysisService;
        
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
            AnalysisTaskType.SURV => await _survSceAnalysisService.Prepare(Parse<Analysis.Services.Surv.Models.Criteria.Analysis>(task.Data), token),
            AnalysisTaskType.DM => await _dmAnalysisService.Prepare(Parse<Analysis.Services.Dm.Models.Criteria.Analysis>(task.Data), token),
            AnalysisTaskType.PCAM => await _pcamAnalysisService.Prepare(Parse<Analysis.Services.Pcam.Models.Criteria.Analysis>(task.Data), token),
            AnalysisTaskType.DE => await _deAnalysisService.Prepare(Parse<Analysis.Services.De.Models.Criteria.Analysis>(task.Data), token),
            AnalysisTaskType.GAF => await _gafAnalysisService.Prepare(Parse<Analysis.Services.Gaf.Models.Criteria.Analysis>(task.Data), token),
            AnalysisTaskType.SCELL => await _scellDcAnalysisService.Prepare(Parse<Analysis.Services.Scell.Models.Criteria.Analysis>(task.Data), token),
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
