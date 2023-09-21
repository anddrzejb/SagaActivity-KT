using MassTransit;

namespace Saga.Events;

public class InitSagaEvent : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; init; }
    
    public bool InitializeWithAlternativeStep { get; init; }
    
    public bool ForceSchedule { get; init; }
}