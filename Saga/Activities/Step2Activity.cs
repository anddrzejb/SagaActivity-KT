using Automatonymous;
using GreenPipes;
using Saga.Configuration;
using Saga.Events;

namespace Saga.Activities;

public class Step2Activity : Activity<SagaStateData, SagaStep2>
{
    private readonly ILogger<Step2Activity> _logger;

    public Step2Activity(ILogger<Step2Activity> logger)
    {
        _logger = logger;
    }

    public void Probe(ProbeContext context) => context.CreateScope("saga-activity-process-step2");

    public void Accept(StateMachineVisitor visitor) => visitor.Visit(this);

    public async Task Execute(BehaviorContext<SagaStateData, SagaStep2> context, Behavior<SagaStateData, SagaStep2> next)
    {
        _logger.LogInformation("{C}{Time} Saga activity: {Activity} initiated by event {EventName} triggered by {EventType} correlated by {Correlation}",
            SagaStateMachine.Yellow, DateTime.Now, GetType().Name, context.Event.Name, context.Data.GetType().Name, context.Data.CorrelationId);
        await next.Execute(context);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<SagaStateData, SagaStep2, TException> context, Behavior<SagaStateData, SagaStep2> next) where TException : Exception
    {
        return next.Faulted(context);
    }
}