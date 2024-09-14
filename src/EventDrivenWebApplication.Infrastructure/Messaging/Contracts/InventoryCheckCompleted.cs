namespace EventDrivenWebApplication.Infrastructure.Messaging.Contracts;

public class InventoryCheckCompleted
{
    public Guid CorrelationId { get; set; }
    public int InventoryItemId { get; set; }
    public int ProductId { get; set; }
    public bool IsQualityGood { get; set; }
    public DateTime DateTimeInventoryCompleted { get; set; } = DateTime.UtcNow;
}