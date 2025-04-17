using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Unite.Analysis.Web.Services;
using Unite.Analysis.Models;
using Unite.Data.Entities.Tasks.Enums;
using Unite.Essentials.Extensions;

namespace Unite.Analysis.Web.Controllers;

[Route("api/[controller]")]
[Authorize]
public class AnalysisController : Controller
{
    private readonly AnalysisTaskService _analysisTaskService;
    private readonly AnalysisRecordService _analysisRecordService;
    private readonly Analysis.Services.Surv.AnalysisService _survAnalysisService;
    private readonly Analysis.Services.Dm.AnalysisService _dmAnalysisService;
    private readonly Analysis.Services.De.AnalysisService _deAnalysisService;
    private readonly Analysis.Services.Scell.AnalysisService _scellAnalysisService;
    

    public AnalysisController(
        AnalysisTaskService analysisTaskService,
        AnalysisRecordService analysisRecordService,
        Analysis.Services.Surv.AnalysisService survSceAnalysisService,
        Analysis.Services.Dm.AnalysisService dmAnalysisService,
        Analysis.Services.De.AnalysisService deAnalysisService,
        Analysis.Services.Scell.AnalysisService scellAnalysisService)
    {
        _analysisTaskService = analysisTaskService;
        _analysisRecordService = analysisRecordService;
        _survAnalysisService = survSceAnalysisService;
        _dmAnalysisService = dmAnalysisService;
        _deAnalysisService = deAnalysisService;
        _scellAnalysisService = scellAnalysisService;
    }
    
    
    [HttpPost("surv")]
    public async Task<IActionResult> CreateSurvTask([FromBody]TypedAnalysis<Analysis.Services.Surv.Models.Criteria.Analysis> model)
    {
        var entry = GenericAnalysis.From(model);

        model.Data.Id = await _analysisRecordService.Add(entry);

        _analysisTaskService.Create(model.Data.Id, model.Data, AnalysisTaskType.SURV);
        
        return Ok(model.Data.Id);
    }

    [HttpPost("dm")]
    public async Task<IActionResult> CreateDmTask([FromBody]TypedAnalysis<Analysis.Services.Dm.Models.Criteria.Analysis> model)
    {
        var entry = GenericAnalysis.From(model);

        model.Data.Id = await _analysisRecordService.Add(entry);

        _analysisTaskService.Create(model.Data.Id, model.Data, AnalysisTaskType.DM);

        return Ok(model.Data.Id);
    }

    [HttpPost("de")]
    public async Task<IActionResult> CreateDeTask([FromBody]TypedAnalysis<Analysis.Services.De.Models.Criteria.Analysis> model)
    {
        var entry = GenericAnalysis.From(model);

        model.Data.Id = await _analysisRecordService.Add(entry);

        _analysisTaskService.Create(model.Data.Id, model.Data, AnalysisTaskType.DE);
        
        return Ok(model.Data.Id);
    }

    [HttpPost("scell")]
    public async Task<IActionResult> CreateScellTask([FromBody]TypedAnalysis<Analysis.Services.Scell.Models.Criteria.Analysis> model)
    {
        var entry = GenericAnalysis.From(model);

        model.Data.Id = await _analysisRecordService.Add(entry);

        _analysisTaskService.Create(model.Data.Id, model.Data, AnalysisTaskType.SCELL);

        return Ok(model.Data.Id);
    }

    [HttpGet("scell/models")]
    public async Task<IActionResult> GetScellModels()
    {
        using var handler = new HttpClientHandler { UseProxy = true };
        using var client = new HttpClient(handler);

        try
        {
            var response = await client.GetAsync("https://celltypist.cog.sanger.ac.uk/models/models.json");
            var content = await response.Content.ReadAsStringAsync();
            return Ok(content);
        }
        catch
        {
            return BadRequest("Failed to fetch models for 'rnasc-dc' task.");
        }
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> GetTaskStatus(string id)
    {
        var task = _analysisTaskService.Get(id);

        if (task == null)
            return NotFound();
        
        await _analysisRecordService.Update(id, task.StatusTypeId.Value.ToDefinitionString());
        
        return Ok(task.StatusTypeId);
    }

    [HttpGet("{id}/meta")]
    public async Task<IActionResult> GetTaskMetadata(string id)
    {
        var task = _analysisTaskService.Get(id);

        if (task == null)
            return NotFound();

        if (task.AnalysisTypeId == AnalysisTaskType.SURV)
            return Ok(await _survAnalysisService.Load(id));
        else if (task.AnalysisTypeId == AnalysisTaskType.DM)
            return Ok(await _dmAnalysisService.Load(id));
        else if (task.AnalysisTypeId == AnalysisTaskType.DE)
            return Ok(await _deAnalysisService.Load(id));
        else if (task.AnalysisTypeId == AnalysisTaskType.SCELL)
            return Ok(await _scellAnalysisService.Load(id));
        
        return BadRequest("Task analysis type is not supported");
    }

    [HttpGet("{id}/data")]
    public async Task<IActionResult> GetTaskData(string id)
    {
        var task = _analysisTaskService.Get(id);

        if (task == null)
            return NotFound();

        if (task.AnalysisTypeId == AnalysisTaskType.SURV)
            return Ok(await _survAnalysisService.Download(id));
        else if (task.AnalysisTypeId == AnalysisTaskType.DM)
            return Ok(await _dmAnalysisService.Download(id));
        else if (task.AnalysisTypeId == AnalysisTaskType.DE)
            return Ok(await _deAnalysisService.Download(id));
        else if (task.AnalysisTypeId == AnalysisTaskType.SCELL)
            return Ok(await _scellAnalysisService.Download(id));

        return BadRequest("Task analysis type is not supported");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(string id)
    {
        var task = _analysisTaskService.Get(id);

        if (task == null)
            return NotFound();

        var statuses = new TaskStatusType?[] {TaskStatusType.Processed, TaskStatusType.Failed};

        if (!statuses.Contains(task.StatusTypeId))
            return BadRequest("Task can't be deleted while in progress.");

        _analysisTaskService.Delete(task);

        if (task.AnalysisTypeId == AnalysisTaskType.SURV)
            await _survAnalysisService.Delete(id);
        else if (task.AnalysisTypeId == AnalysisTaskType.DM)
            await _dmAnalysisService.Delete(id);
        else if (task.AnalysisTypeId == AnalysisTaskType.DE)
            await _deAnalysisService.Delete(id);
        else if (task.AnalysisTypeId == AnalysisTaskType.SCELL)
            await _scellAnalysisService.Delete(id); 
        
        await _analysisRecordService.Delete(id);

        return Ok();
    }
}
