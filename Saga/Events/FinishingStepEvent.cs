using MassTransit;

namespace Saga.Events;

public class FinishingStepEvent : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; init; }
}