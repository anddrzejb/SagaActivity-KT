using MassTransit;

namespace Saga.Events;

public class SagaStep1 : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; init; }
}