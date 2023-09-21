using MassTransit;

namespace Saga;

public class MassTransitHostedService : IHostedService
{
    readonly IBusControl bus;

    public MassTransitHostedService(IBusControl bus)
    {
        this.bus = bus;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await bus.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await bus.StopAsync(cancellationToken);
    }
}