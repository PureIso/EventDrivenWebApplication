using MassTransit;

namespace EventDrivenWebApplication.Domain.Entities;

public class OrderProcessState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public int CurrentState { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int ProductQuantity { get; set; }
    public decimal Price { get; set; }
    public bool IsQualityGood { get; set; }
    public DateTime DateTimeProductCreated { get; set; } = DateTime.UtcNow;
    public DateTime DateTimeInventoryCheckRequested { get; set; } = DateTime.UtcNow;
    public DateTime DateTimeInventoryCheckCompleted { get; set; } = DateTime.UtcNow;
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
