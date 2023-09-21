using GreenPipes;
using MassTransit;
using Saga.Configuration;

namespace Saga;

public class RetryLoggerFilter<T> : IFilter<ConsumeContext<T>> where T : class
{
    ILogger<RetryLoggerFilter<T>> _logger;
    public RetryLoggerFilter(ILogger<RetryLoggerFilter<T>> logger)
    {
        this._logger = logger;
    }
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        if (context.GetRedeliveryCount() > 0)
        {
            _logger.LogInformation("{C}{Time} REDELIVERING message: {MessageId} of type {ContextMessageType} for the {Retries} time",
                SagaStateMachine.Red, DateTime.Now, context.MessageId, context.Message.GetType().Name, context.GetRedeliveryCount());            
        }        
        
        if (context.GetRetryAttempt() > 0)
        {
            _logger.LogInformation("{C}{Time} RETRYING message: {MessageId} of type {ContextMessageType} for the {Retries} time",
                SagaStateMachine.Red, DateTime.Now, context.MessageId, context.Message.GetType().Name, context.GetRetryAttempt());
        }

        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("retryLogger");
    }
}