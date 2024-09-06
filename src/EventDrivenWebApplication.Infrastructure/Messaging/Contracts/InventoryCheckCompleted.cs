namespace EventDrivenWebApplication.Infrastructure.Messaging.Contracts;

public class InventoryCheckCompleted
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; } 
    public bool IsAvailable { get; set; }
    public int QuantityAvailable { get; set; }
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}