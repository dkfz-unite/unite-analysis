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
    private readonly Analysis.Services.DESeq2.AnalysisService _deseq2AnalysisService;
    private readonly Analysis.Services.SCell.AnalysisService _scellAnalysisService;
    private readonly Analysis.Services.KMeier.AnalysisService _kmeierAnalysisService;
    private readonly Analysis.Services.Meth.AnalysisService _methAnalysisService;
    private readonly ILogger _logger;


    public AnalysisProcessingHandler(
        ApiOptions apiOptions,
        AnalysisTaskService analysisTaskService,
        Analysis.Services.DESeq2.AnalysisService deseq2AnalysisService,
        Analysis.Services.SCell.AnalysisService scellAnalysisService,
        Analysis.Services.KMeier.AnalysisService kmeierAnalysisService,
        Analysis.Services.Meth.AnalysisService methAnalysisService,
        ILogger<AnalysisProcessingHandler> logger)
    {
        _apiOptions = apiOptions;
        _analysisTaskService = analysisTaskService;
        _deseq2AnalysisService = deseq2AnalysisService;
        _scellAnalysisService = scellAnalysisService;
        _kmeierAnalysisService = kmeierAnalysisService;
        _methAnalysisService = methAnalysisService;
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
            AnalysisTaskType.DESEQ2 => await _deseq2AnalysisService.Process(task.Target, token),
            AnalysisTaskType.SCELL => await _scellAnalysisService.Process(task.Target, token),
            AnalysisTaskType.KMEIER => await _kmeierAnalysisService.Process(task.Target, token),
            AnalysisTaskType.METH => await _methAnalysisService.Process(task.Target, token),
            _ => throw new NotImplementedException($"Analysis task '{task.AnalysisTypeId}' is not supported")
        };

        if (result.Status == AnalysisTaskStatus.Success)
            _logger.LogInformation("Analysis task '{id}' ({type}) - processed in {elapsed}s", task.Id, task.AnalysisTypeId, result.Elapsed);
        else if (result.Status == AnalysisTaskStatus.Failed)
            _logger.LogError("Analysis task '{id}' ({type}) - failed", task.Id, task.AnalysisTypeId);

        return (byte)result.Status;
    }
}
