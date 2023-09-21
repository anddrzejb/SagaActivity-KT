using Automatonymous;
using GreenPipes;
using Saga.Configuration;
using Saga.Events;

namespace Saga.Activities;

public class InitializationActivity: Activity<SagaStateData, InitSagaEvent>
{
    private readonly ILogger<InitializationActivity> _logger;

    public InitializationActivity(ILogger<InitializationActivity> logger)
    {
        _logger = logger;
    }

    public void Probe(ProbeContext context) => context.CreateScope("saga-activity-process-initialization");

    public void Accept(StateMachineVisitor visitor) => visitor.Visit(this);

    public async Task Execute(BehaviorContext<SagaStateData, InitSagaEvent> context, Behavior<SagaStateData, InitSagaEvent> next)
    {
        _logger.LogInformation("{C}{Time} Saga activity: {Activity} initiated by event {EventName} triggered by {EventType} correlated by {Correlation}",
            SagaStateMachine.Yellow, DateTime.Now, GetType().Name, context.Event.Name, context.Data.GetType().Name, context.Data.CorrelationId);
        await context.Publish(new SagaStep1() { CorrelationId = context.Data.CorrelationId });
        await next.Execute(context);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<SagaStateData, InitSagaEvent, TException> context, Behavior<SagaStateData, InitSagaEvent> next) where TException : Exception
    {
        return next.Faulted(context);
    }
}