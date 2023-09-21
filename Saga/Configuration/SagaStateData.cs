using Automatonymous;
using MassTransit.Saga;

namespace Saga.Configuration;

public class SagaStateData : SagaStateMachineInstance, ISagaVersion
{
    public Guid CorrelationId { get; set; }
    
    public string CurrentState { get; set; } = default!;
    
    public int Version { get; set; }
}