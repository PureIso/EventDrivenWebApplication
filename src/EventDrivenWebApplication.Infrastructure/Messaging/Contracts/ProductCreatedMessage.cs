namespace EventDrivenWebApplication.Infrastructure.Messaging.Contracts;

public class ProductCreatedMessage
{
    // CorrelationId used by the state machine
    public Guid CorrelationId { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public DateTime DateTimeCreated { get; set; } = DateTime.UtcNow;
}