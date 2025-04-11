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
    private readonly Analysis.Services.DonSce.AnalysisService _donSceAnalysisService;
    private readonly Analysis.Services.MethDm.AnalysisService _methDmAnalysisService;
    private readonly Analysis.Services.RnaDe.AnalysisService _rnaDeAnalysisService;
    private readonly Analysis.Services.RnascDc.AnalysisService _rnascDcAnalysisService;


    public AnalysesController(
        AnalysisTaskService analysisTaskService, 
        AnalysisRecordsService analysisRecordsService,
        Analysis.Services.DonSce.AnalysisService donSceAnalysisService,
        Analysis.Services.MethDm.AnalysisService methDmAnalysisService,
        Analysis.Services.RnaDe.AnalysisService rnaDeAnalysisService, 
        Analysis.Services.RnascDc.AnalysisService rnascDcAnalysisService)
    {
        _analysisTaskService = analysisTaskService;
        _analysisRecordsService = analysisRecordsService;
        _donSceAnalysisService = donSceAnalysisService;
        _methDmAnalysisService = methDmAnalysisService;
        _rnaDeAnalysisService = rnaDeAnalysisService;
        _rnascDcAnalysisService = rnascDcAnalysisService;
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

            if (task.AnalysisTypeId == Data.Entities.Tasks.Enums.AnalysisTaskType.DON_SCE)
                await _donSceAnalysisService.Delete(entry.Id);
            else if (task.AnalysisTypeId == Data.Entities.Tasks.Enums.AnalysisTaskType.METH_DM)
                await _methDmAnalysisService.Delete(entry.Id);
            else if (task.AnalysisTypeId == Data.Entities.Tasks.Enums.AnalysisTaskType.RNA_DE)
                await _rnaDeAnalysisService.Delete(entry.Id);
            else if (task.AnalysisTypeId == Data.Entities.Tasks.Enums.AnalysisTaskType.RNASC_DC)
                await _rnascDcAnalysisService.Delete(entry.Id);
        }

        await _analysisRecordsService.Delete(model);

        return Ok();
    }
}
