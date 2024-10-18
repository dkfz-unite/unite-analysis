using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Unite.Analysis.Web.Services;
using Unite.Data.Entities.Tasks.Enums;

namespace Unite.Analysis.Web.Controllers;

[Route("api/[controller]")]
[Authorize]
public class TaskController : Controller
{
    private readonly AnalysisTaskService _analysisTaskService;
    private readonly Analysis.Services.DESeq2.AnalysisService _deseq2DeAnalysisService;
    private readonly Analysis.Services.SCell.AnalysisService _scellAnalysisService;
    private readonly Analysis.Services.KMeier.AnalysisService _kmeierAnalysisService;

    public TaskController(
        AnalysisTaskService analysisTaskService,
        Analysis.Services.DESeq2.AnalysisService deseq2AnalysisService,
        Analysis.Services.SCell.AnalysisService scellAnalysisService,
        Analysis.Services.KMeier.AnalysisService kmeierAnalysisService)
    {
        _analysisTaskService = analysisTaskService;
        _kmeierAnalysisService = kmeierAnalysisService;
        _deseq2DeAnalysisService = deseq2AnalysisService;
        _scellAnalysisService = scellAnalysisService;
    }

    [HttpPost("deseq2")]
    public IActionResult CreateDESeq2Task([FromBody]Analysis.Services.DESeq2.Models.Analysis model)
    {
        model.Key = Guid.NewGuid().ToString();

        _analysisTaskService.Create(model.Key, model, AnalysisTaskType.DESEQ2);

        return Ok(model.Key);
    }

    [HttpPost("scell")]
    public IActionResult CreateSCellTask([FromBody]Analysis.Services.SCell.Models.Analysis model)
    {
        model.Key = Guid.NewGuid().ToString();

        _analysisTaskService.Create(model.Key, model, AnalysisTaskType.SCELL);

        return Ok(model.Key);
    }

    [HttpPost("kmeier")]
    public IActionResult CreateKmeierTask([FromBody]Analysis.Services.KMeier.Models.Analysis model)
    {
        model.Key = Guid.NewGuid().ToString();

        _analysisTaskService.Create(model.Key, model, AnalysisTaskType.KMEIER);

        return Ok(model.Key);
    }

    [HttpGet("{key}/status")]
    public IActionResult GetTaskStatus(string key)
    {
        var task = _analysisTaskService.Get(key);

        if (task == null)
            return NotFound();

        return Ok(task.StatusTypeId);
    }

    [HttpGet("{key}/meta")]
    public async Task<IActionResult> GetTaskMetadata(string key)
    {
        var task = _analysisTaskService.Get(key);

        if (task == null)
            return NotFound();

        if (task.AnalysisTypeId == AnalysisTaskType.DESEQ2)
            return Ok(await _deseq2DeAnalysisService.Load(key));
        else if (task.AnalysisTypeId == AnalysisTaskType.SCELL)
            return Ok(await _scellAnalysisService.Load(key));
        else if (task.AnalysisTypeId == AnalysisTaskType.KMEIER)
            return Ok(await _kmeierAnalysisService.Load(key));
        
        return BadRequest("Task analysis type is not supported");
    }

    [HttpGet("{key}/data")]
    public async Task<IActionResult> GetTaskData(string key)
    {
        var task = _analysisTaskService.Get(key);

        if (task == null)
            return NotFound();

        if (task.AnalysisTypeId == AnalysisTaskType.DESEQ2)
            return Ok(await _deseq2DeAnalysisService.Download(key));
        else if (task.AnalysisTypeId == AnalysisTaskType.SCELL)
            return Ok(await _scellAnalysisService.Download(key));
        else if (task.AnalysisTypeId == AnalysisTaskType.KMEIER)
            return Ok(await _kmeierAnalysisService.Download(key));

        return BadRequest("Task analysis type is not supported");
    }

    [HttpDelete("{key}")]
    public async Task<IActionResult> DeleteTask(string key)
    {
        var task = _analysisTaskService.Get(key);
        var statuses = new TaskStatusType?[] {TaskStatusType.Processed, TaskStatusType.Failed};

        if (task == null)
            return NotFound();

        if (!statuses.Contains(task.StatusTypeId))
            return BadRequest("Task can't be deleted");

        if (task.AnalysisTypeId == AnalysisTaskType.DESEQ2)
            await _deseq2DeAnalysisService.Delete(key);
        else if (task.AnalysisTypeId == AnalysisTaskType.SCELL)
            await _scellAnalysisService.Delete(key);
        else if (task.AnalysisTypeId == AnalysisTaskType.KMEIER)
            await _kmeierAnalysisService.Delete(key);

        _analysisTaskService.Delete(task);

        return Ok();
    }
}
