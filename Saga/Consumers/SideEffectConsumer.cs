using MassTransit;
using Saga.Configuration;
using Saga.Requests;

namespace Saga.Consumers;

public class SideEffectConsumer : IConsumer<SideEffectRequest>
{
    private ILogger<SideEffectConsumer> _logger;

    public SideEffectConsumer(ILogger<SideEffectConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<SideEffectRequest> context)
    {
        _logger.LogInformation("{C}{Time} Consumer: {Consumer} initiated by request {@Request}",
            SagaStateMachine.Red, DateTime.Now, GetType().Name, context.Message);
        return Task.CompletedTask;
    }
}