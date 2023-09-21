using MassTransit;

namespace Saga.Events;

public class AlternativeStep : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; init; }
}