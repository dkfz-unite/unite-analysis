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
    private readonly Analysis.Services.DESeq2.AnalysisService _deseq2AnalysisService;
    private readonly Analysis.Services.SCell.AnalysisService _scellAnalysisService;
    private readonly Analysis.Services.KMeier.AnalysisService _kmeierAnalysisService;
    private readonly ILogger _logger;


    public AnalysisPreparingHandler(
        ApiOptions apiOptions,
        AnalysisTaskService analysisTaskService,
        Analysis.Services.DESeq2.AnalysisService deseq2AnalysisService,
        Analysis.Services.SCell.AnalysisService scellAnalysisService,
        Analysis.Services.KMeier.AnalysisService kmeierAnalysisService,
        ILogger<AnalysisPreparingHandler> logger)
    {
        _apiOptions = apiOptions;
        _analysisTaskService = analysisTaskService;
        _deseq2AnalysisService = deseq2AnalysisService;
        _scellAnalysisService = scellAnalysisService;
        _kmeierAnalysisService = kmeierAnalysisService;
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
            AnalysisTaskType.DESEQ2 => await _deseq2AnalysisService.Prepare(Parse<Analysis.Services.DESeq2.Models.Analysis>(task.Data), token),
            AnalysisTaskType.SCELL => await _scellAnalysisService.Prepare(Parse<Analysis.Services.SCell.Models.Analysis>(task.Data), token),
            AnalysisTaskType.KMEIER => await _kmeierAnalysisService.Prepare(Parse<Analysis.Services.KMeier.Models.Analysis>(task.Data), token),
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
