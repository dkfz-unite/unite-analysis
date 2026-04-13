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
    private readonly Analysis.Services.Pcam.AnalysisService _pcamAnalysisService;
    private readonly Analysis.Services.Deg.AnalysisService _degAnalysisService;
    private readonly Analysis.Services.Gaf.AnalysisService _gafAnalysisService;
    private readonly Analysis.Services.Dep.AnalysisService _depAnalysisService;
    private readonly Analysis.Services.Umapp.AnalysisService _umappAnalysisService;
    private readonly Analysis.Services.Scell.AnalysisService _scellAnalysisService;
    

    public AnalysisController(
        AnalysisTaskService analysisTaskService,
        AnalysisRecordService analysisRecordService,
        Analysis.Services.Surv.AnalysisService survSceAnalysisService,
        Analysis.Services.Dm.AnalysisService dmAnalysisService,
        Analysis.Services.Pcam.AnalysisService pcamAnalysisService,
        Analysis.Services.Deg.AnalysisService degAnalysisService,
        Analysis.Services.Gaf.AnalysisService gafAnalysisService,
        Analysis.Services.Dep.AnalysisService depAnalysisService,
        Analysis.Services.Umapp.AnalysisService umappAnalysisService,
        Analysis.Services.Scell.AnalysisService scellAnalysisService)
    {
        _analysisTaskService = analysisTaskService;
        _analysisRecordService = analysisRecordService;
        _survAnalysisService = survSceAnalysisService;
        _dmAnalysisService = dmAnalysisService;
        _pcamAnalysisService = pcamAnalysisService;
        _degAnalysisService = degAnalysisService;
        _gafAnalysisService = gafAnalysisService;
        _depAnalysisService = depAnalysisService;
        _umappAnalysisService = umappAnalysisService;
        _scellAnalysisService = scellAnalysisService;
    }
    
    
    [HttpPost("surv")]
    public async Task<IActionResult> CreateSurvTask([FromBody]TypedAnalysis<Analysis.Services.Surv.Models.Criteria.Analysis> model)
    {
        return await RunTask(AnalysisTaskType.SURV, model);
    }

    [HttpPost("dm")]
    public async Task<IActionResult> CreateDmTask([FromBody]TypedAnalysis<Analysis.Services.Dm.Models.Criteria.Analysis> model)
    {
        return await RunTask(AnalysisTaskType.DM, model);
    }

    [HttpPost("pcam")]
    public async Task<IActionResult> CreatePcamTask([FromBody]TypedAnalysis<Analysis.Services.Pcam.Models.Criteria.Analysis> model)
    {
        return await RunTask(AnalysisTaskType.PCAM, model);
    }

    [HttpPost("deg")]
    public async Task<IActionResult> CreateDegTask([FromBody]TypedAnalysis<Analysis.Services.Deg.Models.Criteria.Analysis> model)
    {
        return await RunTask(AnalysisTaskType.DEG, model);
    }

    [HttpPost("gaf")]
    public async Task<IActionResult> CreateGafTask([FromBody]TypedAnalysis<Analysis.Services.Gaf.Models.Criteria.Analysis> model)
    {
        return await RunTask(AnalysisTaskType.GAF, model);
    }

    [HttpPost("dep")]
    public async Task<IActionResult> CreateDepTask([FromBody]TypedAnalysis<Analysis.Services.Dep.Models.Criteria.Analysis> model)
    {
        return await RunTask(AnalysisTaskType.DEP, model);
    }

    [HttpPost("umapp")]
    public async Task<IActionResult> CreateUmappTask([FromBody]TypedAnalysis<Analysis.Services.Umapp.Models.Criteria.Analysis> model)
    {
        return await RunTask(AnalysisTaskType.UMAPP, model);
    }

    [HttpPost("scell")]
    public async Task<IActionResult> CreateScellTask([FromBody]TypedAnalysis<Analysis.Services.Scell.Models.Criteria.Analysis> model)
    {
        return await RunTask(AnalysisTaskType.SCELL, model);
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
    public async Task<IActionResult> GetTaskMetadata(string id, [FromQuery] string file)
    {
        var task = _analysisTaskService.Get(id);

        if (task == null)
            return NotFound();

        if (task.AnalysisTypeId == AnalysisTaskType.SURV)
            return Ok(await _survAnalysisService.Load(id, file));
        else if (task.AnalysisTypeId == AnalysisTaskType.DM)
            return Ok(await _dmAnalysisService.Load(id, file));
        else if (task.AnalysisTypeId == AnalysisTaskType.PCAM)
            return Ok(await _pcamAnalysisService.Load(id, file));
        else if (task.AnalysisTypeId == AnalysisTaskType.DEG)
            return Ok(await _degAnalysisService.Load(id, file));
        else if (task.AnalysisTypeId == AnalysisTaskType.GAF)
            return Ok(await _gafAnalysisService.Load(id, file));
        else if (task.AnalysisTypeId == AnalysisTaskType.DEP)
            return Ok(await _depAnalysisService.Load(id, file));
        else if (task.AnalysisTypeId == AnalysisTaskType.UMAPP)
            return Ok(await _umappAnalysisService.Load(id, file));
        else if (task.AnalysisTypeId == AnalysisTaskType.SCELL)
            return Ok(await _scellAnalysisService.Load(id, file));
        
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
        else if (task.AnalysisTypeId == AnalysisTaskType.PCAM)
            return Ok(await _pcamAnalysisService.Download(id));
        else if (task.AnalysisTypeId == AnalysisTaskType.DEG)
            return Ok(await _degAnalysisService.Download(id));
        else if (task.AnalysisTypeId == AnalysisTaskType.GAF)
            return Ok(await _gafAnalysisService.Download(id));
        else if (task.AnalysisTypeId == AnalysisTaskType.DEP)
            return Ok(await _depAnalysisService.Download(id));
        else if (task.AnalysisTypeId == AnalysisTaskType.UMAPP)
            return Ok(await _umappAnalysisService.Download(id));
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
        else if (task.AnalysisTypeId == AnalysisTaskType.PCAM)
            await _pcamAnalysisService.Delete(id);
        else if (task.AnalysisTypeId == AnalysisTaskType.DEG)
            await _degAnalysisService.Delete(id);
        else if (task.AnalysisTypeId == AnalysisTaskType.GAF)
            await _gafAnalysisService.Delete(id);
        else if (task.AnalysisTypeId == AnalysisTaskType.DEP)
            await _depAnalysisService.Delete(id);
        else if (task.AnalysisTypeId == AnalysisTaskType.UMAPP)
            await _umappAnalysisService.Delete(id);
        else if (task.AnalysisTypeId == AnalysisTaskType.SCELL)
            await _scellAnalysisService.Delete(id); 
        
        await _analysisRecordService.Delete(id);

        return Ok();
    }


    private async Task<IActionResult> RunTask<T>(AnalysisTaskType type, TypedAnalysis<T> model) where T : AnalysisData
    {
        var entry = GenericAnalysis.From(model);

        model.Data.Id = await _analysisRecordService.Add(entry);

        _analysisTaskService.Create(model.Data.Id, model.Data, type);

        return Ok(model.Data.Id);
    }
}
