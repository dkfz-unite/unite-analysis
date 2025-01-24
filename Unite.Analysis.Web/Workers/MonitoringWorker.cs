using Unite.Analysis.Web.Handlers;
using Unite.Essentials.Extensions;

namespace Unite.Analysis.Web.Workers;

public class MonitoringWorker : BackgroundService
{
    private readonly MonitoringHandler _handler;
    private readonly ILogger _logger;


    public MonitoringWorker(
        MonitoringHandler handler,
        ILogger<MonitoringWorker> logger)
    {
        _handler = handler;
        _logger = logger;
    }


    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Monitoring service started");

        cancellationToken.Register(() => _logger.LogInformation("Monitoring service stopped"));

        // Delay 5 seconds to let the web api start working
        await Task.Delay(5000, cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _handler.Handle();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.GetShortMessage());
            }
            finally
            {
                await Task.Delay(10000, cancellationToken);
            }
        }
    }
}
