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
    private readonly Analysis.Services.DonSce.AnalysisService _donSceAnalysisService;
    private readonly Analysis.Services.MethDm.AnalysisService _methDmAnalysisService;
    private readonly Analysis.Services.RnaDe.AnalysisService _rnaDeAnalysisService;
    private readonly Analysis.Services.RnascDc.AnalysisService _rnascDcAnalysisService;


    public AnalysisController(
        AnalysisTaskService analysisTaskService,
        AnalysisRecordService analysisRecordService,
        Analysis.Services.DonSce.AnalysisService donSceAnalysisService,
        Analysis.Services.MethDm.AnalysisService methDmAnalysisService,
        Analysis.Services.RnaDe.AnalysisService rnaDeAnalysisService,
        Analysis.Services.RnascDc.AnalysisService rnascDcAnalysisService)
    {
        _analysisTaskService = analysisTaskService;
        _analysisRecordService = analysisRecordService;
        _donSceAnalysisService = donSceAnalysisService;
        _rnaDeAnalysisService = rnaDeAnalysisService;
        _rnascDcAnalysisService = rnascDcAnalysisService;
        _methDmAnalysisService = methDmAnalysisService;
    }
    
    
    [HttpPost("don-sce")]
    public async Task<IActionResult> CreateDonSceTask([FromBody]TypedAnalysis<Analysis.Services.DonSce.Models.Criteria.Analysis> model)
    {
        var entry = GenericAnalysis.From(model);

        model.Data.Id = await _analysisRecordService.Add(entry);

        _analysisTaskService.Create(model.Data.Id, model.Data, AnalysisTaskType.DON_SCE);
        
        return Ok(model.Data.Id);
    }

    [HttpPost("meth-dm")]
    public async Task<IActionResult> CreateMethDmTask([FromBody]TypedAnalysis<Analysis.Services.MethDm.Models.Criteria.Analysis> model)
    {
        var entry = GenericAnalysis.From(model);

        model.Data.Id = await _analysisRecordService.Add(entry);

        _analysisTaskService.Create(model.Data.Id, model.Data, AnalysisTaskType.METH_DM);

        return Ok(model.Data.Id);
    }

    [HttpPost("rna-de")]
    public async Task<IActionResult> CreateRnaDeTask([FromBody]TypedAnalysis<Analysis.Services.RnaDe.Models.Criteria.Analysis> model)
    {
        var entry = GenericAnalysis.From(model);

        model.Data.Id = await _analysisRecordService.Add(entry);

        _analysisTaskService.Create(model.Data.Id, model.Data, AnalysisTaskType.RNA_DE);
        
        return Ok(model.Data.Id);
    }

    [HttpPost("rnasc-dc")]
    public async Task<IActionResult> CreateRnascDcTask([FromBody]TypedAnalysis<Analysis.Services.RnascDc.Models.Criteria.Analysis> model)
    {
        var entry = GenericAnalysis.From(model);

        model.Data.Id = await _analysisRecordService.Add(entry);

        _analysisTaskService.Create(model.Data.Id, model.Data, AnalysisTaskType.RNASC_DC);

        return Ok(model.Data.Id);
    }

    [HttpGet("rnasc-dc/models")]
    public async Task<IActionResult> GetRnascDcModels()
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

        if (task.AnalysisTypeId == AnalysisTaskType.DON_SCE)
            return Ok(await _donSceAnalysisService.Load(id));
        else if (task.AnalysisTypeId == AnalysisTaskType.METH_DM)
            return Ok(await _methDmAnalysisService.Load(id));
        else if (task.AnalysisTypeId == AnalysisTaskType.RNA_DE)
            return Ok(await _rnaDeAnalysisService.Load(id));
        else if (task.AnalysisTypeId == AnalysisTaskType.RNASC_DC)
            return Ok(await _rnascDcAnalysisService.Load(id));
        
        return BadRequest("Task analysis type is not supported");
    }

    [HttpGet("{id}/data")]
    public async Task<IActionResult> GetTaskData(string id)
    {
        var task = _analysisTaskService.Get(id);

        if (task == null)
            return NotFound();

        if (task.AnalysisTypeId == AnalysisTaskType.DON_SCE)
            return Ok(await _donSceAnalysisService.Download(id));
        else if (task.AnalysisTypeId == AnalysisTaskType.METH_DM)
            return Ok(await _methDmAnalysisService.Download(id));
        else if (task.AnalysisTypeId == AnalysisTaskType.RNA_DE)
            return Ok(await _rnaDeAnalysisService.Download(id));
        else if (task.AnalysisTypeId == AnalysisTaskType.RNASC_DC)
            return Ok(await _rnascDcAnalysisService.Download(id));

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

        if (task.AnalysisTypeId == AnalysisTaskType.DON_SCE)
            await _donSceAnalysisService.Delete(id);
        else if (task.AnalysisTypeId == AnalysisTaskType.METH_DM)
            await _methDmAnalysisService.Delete(id);
        if (task.AnalysisTypeId == AnalysisTaskType.RNA_DE)
            await _rnaDeAnalysisService.Delete(id);
        else if (task.AnalysisTypeId == AnalysisTaskType.RNASC_DC)
            await _rnascDcAnalysisService.Delete(id);

        await _analysisRecordService.Delete(id);

        return Ok();
    }
}
