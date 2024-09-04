using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Unite.Analysis.Web.Services;
using Unite.Data.Entities.Tasks.Enums;

namespace Unite.Analysis.Web.Controllers.Analysis;

[Route("api/[controller]")]
[Authorize]
public class TasksController : Controller
{
    private readonly AnalysisTaskService _analysisTaskService;
    private readonly Unite.Analysis.Services.RnaDe.AnalysisService _expressionAnalysisService;
    private readonly Unite.Analysis.Services.Rnasc.AnalysisService _scAnalysisService;

    public TasksController(
        AnalysisTaskService analysisTaskService,
        Unite.Analysis.Services.RnaDe.AnalysisService expressionAnalysisService,
        Unite.Analysis.Services.Rnasc.AnalysisService scAnalysisService)
    {
        _analysisTaskService = analysisTaskService;
        _expressionAnalysisService = expressionAnalysisService;
        _scAnalysisService = scAnalysisService;
    }

    [HttpPost("rna-de")]
    public IActionResult CreateDExpTask([FromBody]Unite.Analysis.Services.RnaDe.Models.Analysis model)
    {
        model.Key = Guid.NewGuid().ToString();

        _analysisTaskService.Create(model.Key, model, AnalysisTaskType.RNA_DE);

        return Ok(model.Key);
    }

    [HttpPost("rnasc")]
    public IActionResult CreateScTask([FromBody]Unite.Analysis.Services.Rnasc.Models.Analysis model)
    {
        model.Key = Guid.NewGuid().ToString();

        _analysisTaskService.Create(model.Key, model, AnalysisTaskType.RNASC);

        return Ok(model.Key);
    }

    [HttpGet("{key}")]
    public IActionResult GetTaskStatus(string key)
    {
        var task = _analysisTaskService.Get(key);

        if (task == null)
            return NotFound();

        return Ok(task.StatusTypeId);
    }

    [HttpGet("{key}/results")]
    public async Task<IActionResult> GetTaskResults(string key)
    {
        var task = _analysisTaskService.Get(key);

        if (task == null)
            return NotFound();

        if (task.AnalysisTypeId == AnalysisTaskType.RNA_DE)
            return Ok(await _expressionAnalysisService.Load(key));
        else if (task.AnalysisTypeId == AnalysisTaskType.RNASC)
            return Ok(await _scAnalysisService.Load(key));
        
        return BadRequest("Task analysis type is not supported");
    }

    [HttpGet("{key}/data")]
    public async Task<IActionResult> GetTaskData(string key)
    {
        var task = _analysisTaskService.Get(key);

        if (task == null)
            return NotFound();

        if (task.AnalysisTypeId == AnalysisTaskType.RNA_DE)
            return Ok(await _expressionAnalysisService.Download(key));
        else if (task.AnalysisTypeId == AnalysisTaskType.RNASC)
            return Ok(await _scAnalysisService.Download(key));

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
            await _expressionAnalysisService.Delete(key);
        else if (task.AnalysisTypeId == AnalysisTaskType.RNASC)
            await _scAnalysisService.Delete(key);

        _analysisTaskService.Delete(task);

        return Ok();
    }
}
