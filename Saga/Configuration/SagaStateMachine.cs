using Automatonymous;
using MassTransit;
using Saga.Events;

namespace Saga.Configuration;

public class SagaStateMachine: MassTransitStateMachine<SagaStateData>
{
    const string Purple = "\x1b[35m";
    public SagaStateMachine(ILogger<SagaStateMachine> logger)
    {
        this.InstanceState(x => x.CurrentState);
        
        this.Initially(
            this.When(InitSaga)
                .TransitionTo(this.Initialized)
                .Then(ctx =>
                {
                    logger.LogInformation("{C}{Time} Saga: Transitioned to {CurrentState} from Start on event {EventName} triggered by {EventType}",
                        Purple, DateTime.Now, ctx.Instance.CurrentState, ctx.Event.Name, ctx.Data.GetType().Name);
                })
                .PublishAsync(ctx => ctx.Init<SagaStep1>(
                    new SagaStep1() { CorrelationId = ctx.Data.CorrelationId }))
        );
        
        this.During(this.Initialized,
            this.When(this.Step1)
                .TransitionTo(this.Step1Completed)
                .Then(ctx =>
                {
                    logger.LogInformation("{C}{Time} Saga: Transitioned to {CurrentState} from Start on event {EventName} triggered by {EventType}",
                        Purple, DateTime.Now, ctx.Instance.CurrentState, ctx.Event.Name, ctx.Data.GetType().Name);
                })
                .PublishAsync(ctx => ctx.Init<SagaStep2>(
                    new SagaStep2() { CorrelationId = ctx.Data.CorrelationId }))            
        );
        
        
        this.During(this.Step1Completed,
            this.When(this.Step2)
                .TransitionTo(this.Step2Completed)
                .Then(ctx =>
                {
                    logger.LogInformation("{C}{Time} Saga: Transitioned to {CurrentState} from Start on event {EventName} triggered by {EventType}",
                        Purple, DateTime.Now, ctx.Instance.CurrentState, ctx.Event.Name, ctx.Data.GetType().Name);
                })
                .PublishAsync(ctx => ctx.Init<FinishingStepEvent>(
                    new FinishingStepEvent() { CorrelationId = ctx.Data.CorrelationId }))            
        );

        this.During(this.Step2Completed,
            this.When(FinishingStep)
                .Finalize()
                .Then(ctx =>
                {
                    logger.LogInformation("{C}{Time} Saga: Transitioned to {CurrentState} from Start on event {EventName} triggered by {EventType}",
                        Purple, DateTime.Now, ctx.Instance.CurrentState, ctx.Event.Name, ctx.Data.GetType().Name);
                })
        );
    }    
    
    public State Initialized { get; private set; }
    
    public State Step1Completed { get; private set; }
    
    public State Step2Completed { get; private set; }
    
    public Event<InitSagaEvent> InitSaga { get; private set; }
    
    public Event<SagaStep1> Step1 { get; private set; }
    
    public Event<SagaStep2> Step2 { get; private set; }
    
    public Event<FinishingStepEvent> FinishingStep { get; private set; }
}