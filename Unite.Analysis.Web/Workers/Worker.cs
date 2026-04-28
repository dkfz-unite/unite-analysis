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
            using var scope = _scopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<THandler>();
            handler.Prepare();
        }
        catch (Exception exception)
        {
            _logger.LogError("{error}", exception.GetShortMessage());
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<THandler>();
                handler.Handle();
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
}
