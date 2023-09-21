using Automatonymous;
using GreenPipes;
using Saga.Configuration;
using Saga.Events;

namespace Saga.Activities;

public class Step1Activity : Activity<SagaStateData, SagaStep1>
{
    private readonly ILogger<Step1Activity> _logger;

    public Step1Activity(ILogger<Step1Activity> logger)
    {
        _logger = logger;
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("saga-activity-process-step1");
    }

    public void Accept(StateMachineVisitor visitor) => visitor.Visit(this);

    public async Task Execute(BehaviorContext<SagaStateData, SagaStep1> context, Behavior<SagaStateData, SagaStep1> next)
    {
        _logger.LogInformation("{C}{Time} Saga activity: {Activity} initiated by event {EventName} triggered by {EventType} correlated by {Correlation}",
            SagaStateMachine.Yellow, DateTime.Now, GetType().Name, context.Event.Name, context.Data.GetType().Name, context.Data.CorrelationId);
        await next.Execute(context);
    }

    public Task Faulted<TException>(BehaviorExceptionContext<SagaStateData, SagaStep1, TException> context, Behavior<SagaStateData, SagaStep1> next) where TException : Exception
    {
        return next.Faulted(context);
    }
}