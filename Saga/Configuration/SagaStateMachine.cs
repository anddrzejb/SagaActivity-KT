using Automatonymous;
using MassTransit;
using Saga.Activities;
using Saga.Events;
using Saga.Requests;

namespace Saga.Configuration;

public class SagaStateMachine: MassTransitStateMachine<SagaStateData>
{
    public const string Purple = "\x1b[35m";
    public const string Yellow = "\x1b[33m";
    public const string Red = "\x1b[31m";
    public const string Blue = "\x1b[34m";
    public static int RetryCount = 0;
    public SagaStateMachine(ILogger<SagaStateMachine> logger)
    {
        this.InstanceState(x => x.CurrentState);
        
        this.Schedule(() => this.TimeoutSchedule,
            instance => instance.TimeoutScheduleTokenId,
            schedule =>
            {
                schedule.Delay = TimeSpan.FromSeconds(10);
                schedule.Received = r => r.CorrelateById(context => context.Message.CorrelationId);
            });
        
        this.Initially(
            this.When(InitSaga)             
                .Activity(x => x.OfType<InitializationActivity>())
                .TransitionTo(this.Initialized)             
                .Then(ctx =>
                {
                    ctx.Instance.InitializeWithAlternativeStep = ctx.Data.InitializeWithAlternativeStep;
                    
                    logger.LogInformation("{C}{Time} Saga: Transitioned to {CurrentState} from Start on event {EventName} triggered by {EventType}",
                        Purple, DateTime.Now, ctx.Instance.CurrentState, ctx.Event.Name, ctx.Data.GetType().Name);
                })
                .Schedule(this.TimeoutSchedule, ctx =>
                {
                    logger.LogInformation("{C}{Time} Saga: Schedule {ScheduleName} set to execute in {Delay}",
                        Blue, DateTime.Now, nameof(this.TimeoutSchedule), 15);
                    return ctx.Init<TimeoutEvent>(new TimeoutEvent()
                    {
                        CorrelationId = ctx.Data.CorrelationId
                    });
                })
                .PublishAsync(ctx => ctx.Init<SideEffectRequest>(new SideEffectRequest()
                {
                    CorrelationId = ctx.Instance.CorrelationId,
                    CreatedAt = DateTime.Now
                }))
        );
        
        this.During(this.Initialized,
            this.When(this.Step1)
                .Activity(x => x.OfType<Step1Activity>())
                .TransitionTo(this.Step1Completed)
                .Then(ctx =>
                {
                    logger.LogInformation("{C}{Time} Saga: Transitioned to {CurrentState} on event {EventName} triggered by {EventType}",
                        Purple, DateTime.Now, ctx.Instance.CurrentState, ctx.Event.Name, ctx.Data.GetType().Name);
                })
                .IfElse(condition => !condition.Instance.InitializeWithAlternativeStep,
                    then => then.PublishAsync(ctx => ctx.Init<SagaStep2>(
                        new SagaStep2() { CorrelationId = ctx.Data.CorrelationId })),
                    @else => @else.PublishAsync(ctx => ctx.Init<AlternativeStep>(
                            new AlternativeStep() { CorrelationId = ctx.Data.CorrelationId })))
        );
        
        
        this.During(this.Step1Completed,
            this.When(this.Step2)
                .Activity(x => x.OfType<Step2Activity>())
                .TransitionTo(this.Step2Completed)
                .Then(ctx =>
                {
                    logger.LogInformation("{C}{Time} Saga: Transitioned to {CurrentState} on event {EventName} triggered by {EventType}",
                        Purple, DateTime.Now, ctx.Instance.CurrentState, ctx.Event.Name, ctx.Data.GetType().Name);
                })
                .PublishAsync(ctx => ctx.Init<FinishingStepEvent>(
                    new FinishingStepEvent() { CorrelationId = ctx.Data.CorrelationId })),
            this.When(this.AlternativeStep)
                .TransitionTo(this.Step2Completed)
                .Then(ctx =>
                {
                    logger.LogInformation("{C}{Time} Saga: Transitioned to {CurrentState} on event {EventName} triggered by {EventType}",
                        Purple, DateTime.Now, ctx.Instance.CurrentState, ctx.Event.Name, ctx.Data.GetType().Name);
                })
                .PublishAsync(ctx => ctx.Init<FinishingStepEvent>(
                    new FinishingStepEvent() { CorrelationId = ctx.Data.CorrelationId }))
        );

        this.During(this.Step2Completed,
            this.When(FinishingStep)
                .Unschedule(this.TimeoutSchedule)
                .Then(ctx =>
                {
                    logger.LogInformation("{C}{Time} Saga: Schedule {ScheduleName} has been unscheduled",
                        Blue, DateTime.Now, nameof(this.TimeoutSchedule));
                })                
                .Finalize()
                .Then(ctx =>
                {
                    logger.LogInformation("{C}{Time} Saga: Transitioned to {CurrentState} on event {EventName} triggered by {EventType}",
                        Purple, DateTime.Now, ctx.Instance.CurrentState, ctx.Event.Name, ctx.Data.GetType().Name);
                })
        );
        
        this.DuringAny(this.When(TimeoutSchedule!.Received)
            .Finalize()
            .Then(ctx =>
            {
                logger.LogInformation("{C}{Time} Saga: Transitioned to {CurrentState} on scheduled event {EventName} triggered by {EventType}",
                    Blue, DateTime.Now, ctx.Instance.CurrentState, ctx.Event.Name, ctx.Data.GetType().Name);
            })
        );
    }    
    
    public State Initialized { get; private set; }
    
    public State Step1Completed { get; private set; }
    
    public State Step2Completed { get; private set; }
    
    public Event<InitSagaEvent> InitSaga { get; private set; }
    
    public Event<SagaStep1> Step1 { get; private set; }
    
    public Event<SagaStep2> Step2 { get; private set; }
    
    public Event<AlternativeStep> AlternativeStep { get; private set; }    
    
    public Event<FinishingStepEvent> FinishingStep { get; private set; }
    
    public Schedule<SagaStateData, TimeoutEvent> TimeoutSchedule { get; private set; }
}