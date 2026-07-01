using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Unite.Analysis.Web.Services;
using Unite.Analysis.Models;
using Unite.Analysis.Services.CnvProfile;
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
    private readonly Analysis.Services.CnvProfile.AnalysisService _cnvProfileAnalysisService;
    

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
        Analysis.Services.Scell.AnalysisService scellAnalysisService, 
        AnalysisService cnvProfileAnalysisService)
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
        _cnvProfileAnalysisService = cnvProfileAnalysisService;
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
    
    [HttpPost("cnv-profile")]
    public async Task<IActionResult> CreateCnvProfileTask([FromBody]TypedAnalysis<Analysis.Services.CnvProfile.Models.Criteria.Analysis> model)
    {
        return await RunTask(AnalysisTaskType.CNV_PROFILE, model);
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

        return task.AnalysisTypeId switch
        {
            AnalysisTaskType.SURV => Ok(await _survAnalysisService.Load(id, file)),
            AnalysisTaskType.DM => Ok(await _dmAnalysisService.Load(id, file)),
            AnalysisTaskType.PCAM => Ok(await _pcamAnalysisService.Load(id, file)),
            AnalysisTaskType.DEG => Ok(await _degAnalysisService.Load(id, file)),
            AnalysisTaskType.GAF => Ok(await _gafAnalysisService.Load(id, file)),
            AnalysisTaskType.DEP => Ok(await _depAnalysisService.Load(id, file)),
            AnalysisTaskType.UMAPP => Ok(await _umappAnalysisService.Load(id, file)),
            AnalysisTaskType.SCELL => Ok(await _scellAnalysisService.Load(id, file)),
            AnalysisTaskType.CNV_PROFILE => Ok(await _cnvProfileAnalysisService.Load(id, file)),
            _ => BadRequest("Task analysis type is not supported")
        };
    }

    [HttpGet("{id}/data")]
    public async Task<IActionResult> GetTaskData(string id)
    {
        var task = _analysisTaskService.Get(id);

        if (task == null)
            return NotFound();

        return task.AnalysisTypeId switch
        {
            AnalysisTaskType.SURV => Ok(await _survAnalysisService.Download(id)),
            AnalysisTaskType.DM => Ok(await _dmAnalysisService.Download(id)),
            AnalysisTaskType.PCAM => Ok(await _pcamAnalysisService.Download(id)),
            AnalysisTaskType.DEG => Ok(await _degAnalysisService.Download(id)),
            AnalysisTaskType.GAF => Ok(await _gafAnalysisService.Download(id)),
            AnalysisTaskType.DEP => Ok(await _depAnalysisService.Download(id)),
            AnalysisTaskType.UMAPP => Ok(await _umappAnalysisService.Download(id)),
            AnalysisTaskType.SCELL => Ok(await _scellAnalysisService.Download(id)),
            AnalysisTaskType.CNV_PROFILE => Ok(await _cnvProfileAnalysisService.Download(id)),
            _ => BadRequest("Task analysis type is not supported")
        };
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

        switch (task.AnalysisTypeId)
        {
            case AnalysisTaskType.SURV:
                await _survAnalysisService.Delete(id);
                break;
            case AnalysisTaskType.DM:
                await _dmAnalysisService.Delete(id);
                break;
            case AnalysisTaskType.PCAM:
                await _pcamAnalysisService.Delete(id);
                break;
            case AnalysisTaskType.DEG:
                await _degAnalysisService.Delete(id);
                break;
            case AnalysisTaskType.GAF:
                await _gafAnalysisService.Delete(id);
                break;
            case AnalysisTaskType.DEP:
                await _depAnalysisService.Delete(id);
                break;
            case AnalysisTaskType.UMAPP:
                await _umappAnalysisService.Delete(id);
                break;
            case AnalysisTaskType.SCELL:
                await _scellAnalysisService.Delete(id);
                break;
            case AnalysisTaskType.CNV_PROFILE:
                await _cnvProfileAnalysisService.Delete(id);
                break;
        } 
        
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
