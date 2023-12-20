using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Unite.Essentials.Extensions;

namespace Unite.Analysis.Web.Configuration.Filters;

public class DefaultExceptionFilter : IExceptionFilter
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger _logger;

    public DefaultExceptionFilter(
        IWebHostEnvironment environment,
        ILogger<DefaultExceptionFilter> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception.GetShortMessage());

        context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
    }
}
