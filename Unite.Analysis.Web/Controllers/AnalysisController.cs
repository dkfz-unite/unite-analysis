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
    private readonly Analysis.Services.DESeq2.AnalysisService _deseq2DeAnalysisService;
    private readonly Analysis.Services.SCell.AnalysisService _scellAnalysisService;
    private readonly Analysis.Services.KMeier.AnalysisService _kmeierAnalysisService;


    public AnalysisController(
        AnalysisTaskService analysisTaskService,
        AnalysisRecordService analysisRecordService,
        Analysis.Services.DESeq2.AnalysisService deseq2AnalysisService,
        Analysis.Services.SCell.AnalysisService scellAnalysisService,
        Analysis.Services.KMeier.AnalysisService kmeierAnalysisService)
    {
        _analysisTaskService = analysisTaskService;
        _analysisRecordService = analysisRecordService;
        _kmeierAnalysisService = kmeierAnalysisService;
        _deseq2DeAnalysisService = deseq2AnalysisService;
        _scellAnalysisService = scellAnalysisService;
    }
    
    
    [HttpPost("kmeier")]
    public async Task<IActionResult> CreateKmeierTask([FromBody]TypedAnalysis<Analysis.Services.KMeier.Models.Criteria.Analysis> model)
    {
        var entry = GenericAnalysis.From(model);

        model.Data.Id = await _analysisRecordService.Add(entry);

        _analysisTaskService.Create(model.Data.Id, model.Data, AnalysisTaskType.KMEIER);
        
        return Ok(model.Data.Id);
    }

    [HttpPost("deseq2")]
    public async Task<IActionResult> CreateDESeq2Task([FromBody]TypedAnalysis<Analysis.Services.DESeq2.Models.Criteria.Analysis> model)
    {
        var entry = GenericAnalysis.From(model);

        model.Data.Id = await _analysisRecordService.Add(entry);

        _analysisTaskService.Create(model.Data.Id, model.Data, AnalysisTaskType.DESEQ2);
        
        return Ok(model.Data.Id);
    }

    [HttpPost("scell")]
    public async Task<IActionResult> CreateSCellTask([FromBody]TypedAnalysis<Analysis.Services.SCell.Models.Criteria.Analysis> model)
    {
        var entry = GenericAnalysis.From(model);

        model.Data.Id = await _analysisRecordService.Add(entry);

        _analysisTaskService.Create(model.Data.Id, model.Data, AnalysisTaskType.SCELL);

        return Ok(model.Data.Id);
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

        if (task.AnalysisTypeId == AnalysisTaskType.DESEQ2)
            return Ok(await _deseq2DeAnalysisService.Load(id));
        else if (task.AnalysisTypeId == AnalysisTaskType.SCELL)
            return Ok(await _scellAnalysisService.Load(id));
        else if (task.AnalysisTypeId == AnalysisTaskType.KMEIER)
            return Ok(await _kmeierAnalysisService.Load(id));
        
        return BadRequest("Task analysis type is not supported");
    }

    [HttpGet("{id}/data")]
    public async Task<IActionResult> GetTaskData(string id)
    {
        var task = _analysisTaskService.Get(id);

        if (task == null)
            return NotFound();

        if (task.AnalysisTypeId == AnalysisTaskType.DESEQ2)
            return Ok(await _deseq2DeAnalysisService.Download(id));
        else if (task.AnalysisTypeId == AnalysisTaskType.SCELL)
            return Ok(await _scellAnalysisService.Download(id));
        else if (task.AnalysisTypeId == AnalysisTaskType.KMEIER)
            return Ok(await _kmeierAnalysisService.Download(id));

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

        if (task.AnalysisTypeId == AnalysisTaskType.DESEQ2)
            await _deseq2DeAnalysisService.Delete(id);
        else if (task.AnalysisTypeId == AnalysisTaskType.SCELL)
            await _scellAnalysisService.Delete(id);
        else if (task.AnalysisTypeId == AnalysisTaskType.KMEIER)
            await _kmeierAnalysisService.Delete(id);

        await _analysisRecordService.Delete(id);

        return Ok();
    }
}
