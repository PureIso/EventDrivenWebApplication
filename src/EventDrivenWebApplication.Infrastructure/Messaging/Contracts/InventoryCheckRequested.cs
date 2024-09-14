namespace EventDrivenWebApplication.Infrastructure.Messaging.Contracts;

public class InventoryCheckRequested
{
    public Guid CorrelationId { get; set; } 
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public DateTime DateTimeProductCreated { get; set; }
    public DateTime DateTimeInventoryChecked { get; set; }
    public DateTime DateTimeCreated { get; set; } = DateTime.UtcNow;
}