namespace EventDrivenWebApplication.Infrastructure.Messaging.Contracts;

public class ProductCreatedMessage
{
    //Unique identifier for saga correlation
    public Guid CorrelationId { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid OrderId { get; set; }
}