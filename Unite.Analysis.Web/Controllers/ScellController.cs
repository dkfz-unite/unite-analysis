using Microsoft.AspNetCore.Mvc;
using Unite.Analysis.Web.Services;

namespace Unite.Analysis.Web.Controllers;

[Route("api/[controller]")]
public class ScellController : Controller
{
    private readonly IWebHostEnvironment _environment;
    private readonly ScellViewerService _viewerService;


    public ScellController(IWebHostEnvironment environment, ScellViewerService viewerService)
    {
        _environment = environment;
        _viewerService = viewerService;
    }

    
    /// <summary>
    /// Spawns a new single cell viewer instance.
    /// </summary>
    /// <param name="key">Analysis key.</param>
    /// <returns>Created instance number.</returns>
    [HttpPost("{key}")]
    public async Task<IActionResult> Post(string key)
    {
        var local = _environment.IsDevelopment();

        var name = await _viewerService.Spawn(key, local);

        return Ok(name);
    }

    /// <summary>
    /// Kills a single cell viewer instance.
    /// </summary>
    /// <param name="key">Analysis key.</param>
    /// <returns></returns>
    [HttpDelete("{key}")]
    public async Task<IActionResult> Delete(string key)
    {
        await _viewerService.Kill(key);

        return Ok();
    }
}
