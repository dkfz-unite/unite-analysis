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
    private readonly Analysis.Services.RnaDe.AnalysisService _rnaDeAnalysisService;
    private readonly Analysis.Services.Rnasc.AnalysisService _rnascAnalysisService;

    public TaskController(
        AnalysisTaskService analysisTaskService,
        Analysis.Services.RnaDe.AnalysisService rnaDeAnalysisService,
        Analysis.Services.Rnasc.AnalysisService rnascAnalysisService)
    {
        _analysisTaskService = analysisTaskService;
        _rnaDeAnalysisService = rnaDeAnalysisService;
        _rnascAnalysisService = rnascAnalysisService;
    }

    [HttpPost("rna-de")]
    public IActionResult CreateRnaDeTask([FromBody]Analysis.Services.RnaDe.Models.Analysis model)
    {
        model.Key = Guid.NewGuid().ToString();

        _analysisTaskService.Create(model.Key, model, AnalysisTaskType.RNA_DE);

        return Ok(model.Key);
    }

    [HttpPost("rnasc")]
    public IActionResult CreateRnascTask([FromBody]Analysis.Services.Rnasc.Models.Analysis model)
    {
        model.Key = Guid.NewGuid().ToString();

        _analysisTaskService.Create(model.Key, model, AnalysisTaskType.RNASC);

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

        if (task.AnalysisTypeId == AnalysisTaskType.RNA_DE)
            return Ok(await _rnaDeAnalysisService.Load(key));
        else if (task.AnalysisTypeId == AnalysisTaskType.RNASC)
            return Ok(await _rnascAnalysisService.Load(key));
        
        return BadRequest("Task analysis type is not supported");
    }

    [HttpGet("{key}/data")]
    public async Task<IActionResult> GetTaskData(string key)
    {
        var task = _analysisTaskService.Get(key);

        if (task == null)
            return NotFound();

        if (task.AnalysisTypeId == AnalysisTaskType.RNA_DE)
            return Ok(await _rnaDeAnalysisService.Download(key));
        else if (task.AnalysisTypeId == AnalysisTaskType.RNASC)
            return Ok(await _rnascAnalysisService.Download(key));

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

        if (task.AnalysisTypeId == AnalysisTaskType.RNA_DE)
            await _rnaDeAnalysisService.Delete(key);
        else if (task.AnalysisTypeId == AnalysisTaskType.RNASC)
            await _rnascAnalysisService.Delete(key);

        _analysisTaskService.Delete(task);

        return Ok();
    }
}
