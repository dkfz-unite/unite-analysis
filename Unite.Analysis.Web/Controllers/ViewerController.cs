using Microsoft.AspNetCore.Mvc;
using Unite.Orchestrator;
using Unite.Orchestrator.Docker.Cache;

namespace Unite.Analysis.Web.Controllers;

[Route("api/[controller]")]
public class ViewerController : Controller
{
    private readonly IWebHostEnvironment _environment;
    private readonly CxgViewerService _viewerService;


    public ViewerController(IWebHostEnvironment environment, CxgViewerService viewerService)
    {
        _environment = environment;
        _viewerService = viewerService;
    }

    
    /// <summary>
    /// Spawns a new single cell viewer instance.
    /// </summary>
    /// <param name="id">Analysis identifier.</param>
    /// <returns>Created instance number.</returns>
    [HttpPost("scell")]
    public async Task<IActionResult> Post([FromQuery] string id)
    {
        var name = await _viewerService.Spawn(id);

        return Ok(name);
    }

    /// <summary>
    /// Updates a single cell viewer instance.
    /// </summary>
    /// <param name="id">Analysis identifier.</param>
    /// <returns></returns>
    [HttpPut("scell")]
    public async Task<IActionResult> Put([FromQuery] string id)
    {
        if (ContainerRecords.TryGet(id, out var record))
        {
            record.LastActive = DateTime.UtcNow;
            return await Task.FromResult(Ok(true));
        }
        else
        {
            return await Task.FromResult(Ok(false));
        }
    }
}
