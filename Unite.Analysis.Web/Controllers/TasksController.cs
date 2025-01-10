using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Unite.Analysis.Models;
using Unite.Analysis.Web.Services;

namespace Unite.Analysis.Web.Controllers;

[Route("api/[controller]")]
[Authorize]
public class TasksController : Controller
{
    private readonly AnalysisRecordsService _analysisTasksService;

    public TasksController(AnalysisRecordsService analysisTasksService)
    {
        _analysisTasksService = analysisTasksService;
    }

    [HttpPost()]
    public async Task<IEnumerable<GenericAnalysis>> Load([FromBody]GenericAnalysis model)
	{
		return await _analysisTasksService.Load(model.UserId);
	}
}