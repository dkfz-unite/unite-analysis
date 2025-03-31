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
    private readonly Analysis.Services.DESeq2.AnalysisService _deseq2DeAnalysisService;
    private readonly Analysis.Services.SCell.AnalysisService _scellAnalysisService;
    private readonly Analysis.Services.KMeier.AnalysisService _kmeierAnalysisService;
    private readonly Analysis.Services.Meth.AnalysisService _methAnalysisService;


    public AnalysesController(
        AnalysisTaskService analysisTaskService, 
        AnalysisRecordsService analysisRecordsService, 
        Analysis.Services.DESeq2.AnalysisService deseq2DeAnalysisService, 
        Analysis.Services.SCell.AnalysisService scellAnalysisService, 
        Analysis.Services.KMeier.AnalysisService kmeierAnalysisService,
        Analysis.Services.Meth.AnalysisService methAnalysisService)
    {
        _analysisTaskService = analysisTaskService;
        _analysisRecordsService = analysisRecordsService;
        _deseq2DeAnalysisService = deseq2DeAnalysisService;
        _scellAnalysisService = scellAnalysisService;
        _kmeierAnalysisService = kmeierAnalysisService;
        _methAnalysisService = methAnalysisService;
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

            if (task.AnalysisTypeId == Data.Entities.Tasks.Enums.AnalysisTaskType.DESEQ2)
                await _deseq2DeAnalysisService.Delete(entry.Id);
            else if (task.AnalysisTypeId == Data.Entities.Tasks.Enums.AnalysisTaskType.SCELL)
                await _scellAnalysisService.Delete(entry.Id);
            else if (task.AnalysisTypeId == Data.Entities.Tasks.Enums.AnalysisTaskType.KMEIER)
                await _kmeierAnalysisService.Delete(entry.Id);
            else if (task.AnalysisTypeId == Data.Entities.Tasks.Enums.AnalysisTaskType.METH)
                await _methAnalysisService.Delete(entry.Id);
        }

        await _analysisRecordsService.Delete(model);

        return Ok();
    }
}
