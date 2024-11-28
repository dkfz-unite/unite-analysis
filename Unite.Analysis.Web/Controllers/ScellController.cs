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

    [HttpGet("{key}")]
    public async Task<IActionResult> Get(string key)
    {
        var local = _environment.IsDevelopment();

        var success = await _viewerService.Ping(key, local);

        return Ok(success);
    }
    

    [HttpPost("{key}")]
    public async Task<IActionResult> Post(string key)
    {
        var name = await _viewerService.Spawn(key);

        return Ok(name);
    }

    [HttpDelete("{key}")]
    public async Task<IActionResult> Delete(string key)
    {
        await _viewerService.Kill(key);

        return Ok();
    }
}
