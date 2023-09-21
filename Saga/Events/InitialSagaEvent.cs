using MassTransit;

namespace Saga.Events;

public class InitSagaEvent : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; init; }
}