using System.Text.Json;
using Unite.Analysis.Models;
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
    private readonly Analysis.Services.RnaDe.AnalysisService _expressionAnalysisService;
    private readonly Analysis.Services.Rnasc.AnalysisService _scAnalysisService;
    private readonly ILogger _logger;


    public AnalysisPreparingHandler(
        ApiOptions apiOptions,
        AnalysisTaskService analysisTaskService,
        Analysis.Services.RnaDe.AnalysisService expressionAnalysisService,
        Analysis.Services.Rnasc.AnalysisService scAnalysisService,
        ILogger<AnalysisPreparingHandler> logger)
    {
        _apiOptions = apiOptions;
        _analysisTaskService = analysisTaskService;
        _expressionAnalysisService = expressionAnalysisService;
        _scAnalysisService = scAnalysisService;
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
        var result = task.AnalysisTypeId switch
        {
            AnalysisTaskType.RNA_DE => await PrepareRnaDeTask(task.Data),
            AnalysisTaskType.RNASC => await PrepareRnascTask(task.Data),
            _ => throw new NotImplementedException()
        };

        if (result.Status == AnalysisTaskStatus.Success)
            _logger.LogInformation("Analysis task '{id}' ({type}) - prepared in {elapsed}s", task.Id, task.AnalysisTypeId, result.Elapsed);
        else if (result.Status == AnalysisTaskStatus.Failed)
            _logger.LogError("Analysis task '{id}' ({type}) - failed", task.Id, task.AnalysisTypeId);

        return (byte)result.Status;
    }

    private Task<AnalysisTaskResult> PrepareRnaDeTask(string data)
    {
        var token = TokenHelper.Generate(_apiOptions.Key);
        var model = JsonSerializer.Deserialize<Analysis.Services.RnaDe.Models.Analysis>(data);

        return _expressionAnalysisService.Prepare(model, token);
    }

    private Task<AnalysisTaskResult> PrepareRnascTask(string data)
    {
        var token = TokenHelper.Generate(_apiOptions.Key);
        var model = JsonSerializer.Deserialize<Analysis.Services.Rnasc.Models.Analysis>(data);

        return _scAnalysisService.Prepare(model, token);
    }
}
