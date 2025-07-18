using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Unite.Analysis.Web.Services;

namespace Unite.Analysis.Web.Controllers;

[Route("api/[controller]")]
[Authorize]
public class AnalysesController : Controller
{
    private readonly AnalysisTaskService _analysisTaskService;
    private readonly AnalysisRecordsService _analysisRecordsService;
    private readonly Analysis.Services.Surv.AnalysisService _survAnalysisService;
    private readonly Analysis.Services.Dm.AnalysisService _dmAnalysisService;
    private readonly Analysis.Services.Pcam.AnalysisService _pcamAnalysisService;
    private readonly Analysis.Services.De.AnalysisService _deAnalysisService;
    private readonly Analysis.Services.Scell.AnalysisService _scellAnalysisService;


    public AnalysesController(
        AnalysisTaskService analysisTaskService, 
        AnalysisRecordsService analysisRecordsService,
        Analysis.Services.Surv.AnalysisService survAnalysisService,
        Analysis.Services.Dm.AnalysisService dmAnalysisService,
        Analysis.Services.Pcam.AnalysisService pcamAnalysisService,
        Analysis.Services.De.AnalysisService deAnalysisService, 
        Analysis.Services.Scell.AnalysisService scellAnalysisService)
    {
        _analysisTaskService = analysisTaskService;
        _analysisRecordsService = analysisRecordsService;
        _survAnalysisService = survAnalysisService;
        _dmAnalysisService = dmAnalysisService;
        _pcamAnalysisService = pcamAnalysisService;
        _deAnalysisService = deAnalysisService;
        _scellAnalysisService = scellAnalysisService;
    }


    [HttpPost()]
    public async Task<IActionResult> Load([FromBody] SearchModel model)
	{
		var entries = await _analysisRecordsService.Load(model);

        return Json(entries);
	}

    [HttpDelete()]
    public async Task<IActionResult> Delete([FromQuery] string userId)
    {
        // Issue: Analyses should be removed only if they are complete or failed.
        // Skipped temporarily due to proper handling difficulties.

        var model = new SearchModel(userId);
        var entries = await _analysisRecordsService.Load(model);

        foreach (var entry in entries)
        {
            var task = _analysisTaskService.Get(entry.Id);

            if (task == null)
                continue;

            _analysisTaskService.Delete(task);

            if (task.AnalysisTypeId == Data.Entities.Tasks.Enums.AnalysisTaskType.SURV)
                await _survAnalysisService.Delete(entry.Id);
            else if (task.AnalysisTypeId == Data.Entities.Tasks.Enums.AnalysisTaskType.DM)
                await _dmAnalysisService.Delete(entry.Id);
            else if (task.AnalysisTypeId == Data.Entities.Tasks.Enums.AnalysisTaskType.PCAM)
                await _pcamAnalysisService.Delete(entry.Id);
            else if (task.AnalysisTypeId == Data.Entities.Tasks.Enums.AnalysisTaskType.DE)
                await _deAnalysisService.Delete(entry.Id);
            else if (task.AnalysisTypeId == Data.Entities.Tasks.Enums.AnalysisTaskType.SCELL)
                await _scellAnalysisService.Delete(entry.Id);
        }

        await _analysisRecordsService.Delete(model);

        return Ok();
    }
}
