using Unite.Analysis.Web.Handlers;
using Unite.Essentials.Extensions;

namespace Unite.Analysis.Web.Workers;

public abstract class Worker<THandler> : BackgroundService
    where THandler : Handler
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger _logger;


    protected Worker(
        IServiceScopeFactory scopeFactory,
        ILogger logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }


    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Service started");

        cancellationToken.Register(() => _logger.LogInformation("Service stopped"));

        // Delay 5 seconds to let the web api start working
        await Task.Delay(5000, cancellationToken);

        try
        {
            GetHandler().Prepare();
        }
        catch (Exception exception)
        {
            _logger.LogError("{error}", exception.GetShortMessage());
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                GetHandler().Handle();
            }
            catch (Exception exception)
            {
                _logger.LogError("{error}", exception.GetShortMessage());
            }
            finally
            {
                await Task.Delay(1000, cancellationToken);
            }
        }
    }

    private THandler GetHandler()
    {
        using var scope = _scopeFactory.CreateScope();

        return scope.ServiceProvider.GetRequiredService<THandler>();
    }
}
