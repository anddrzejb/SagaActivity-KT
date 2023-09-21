using MassTransit;

namespace Saga.Events;

public class SagaStep2 : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; init; }
}