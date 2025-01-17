using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Unite.Analysis.Models;
using Unite.Analysis.Web.Services;

namespace Unite.Analysis.Web.Controllers;

[Route("api/[controller]")]
[Authorize]
public class TasksController : Controller
{
    private readonly AnalysisRecordsService _analysisRecordsService;

    public TasksController(AnalysisRecordsService analysisRecordsService)
    {
        _analysisRecordsService = analysisRecordsService;
    }

    [HttpPost()]
    public async Task<IEnumerable<GenericAnalysis>> Load([FromBody]GenericAnalysis model)
	{
		return await _analysisRecordsService.Load(model.UserId);
	}
}