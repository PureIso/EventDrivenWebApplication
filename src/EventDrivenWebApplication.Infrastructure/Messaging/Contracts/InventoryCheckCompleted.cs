namespace EventDrivenWebApplication.Infrastructure.Messaging.Contracts;

public class InventoryCheckCompleted
{
    public Guid OrderId { get; set; }
    public bool IsAvailable { get; set; }
}