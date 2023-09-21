namespace Saga.Requests;

public class SideEffectRequest
{
    public Guid CorrelationId { get; init; }
    
    public DateTime CreatedAt { get; init; }
}