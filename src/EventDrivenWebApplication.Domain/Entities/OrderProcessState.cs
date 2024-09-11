using MassTransit;

namespace EventDrivenWebApplication.Domain.Entities;

public class OrderProcessState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public State CurrentState { get; set; }

    // Saga state properties
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public bool IsInventoryAvailable { get; set; }
    public bool IsInventoryChecked { get; set; }
    public DateTime? InventoryCheckedAt { get; set; }

    // RowVersion for concurrency control
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}