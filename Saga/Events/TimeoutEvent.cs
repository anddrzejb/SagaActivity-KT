using MassTransit;

namespace Saga.Events;

public class TimeoutEvent : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; init; }
}