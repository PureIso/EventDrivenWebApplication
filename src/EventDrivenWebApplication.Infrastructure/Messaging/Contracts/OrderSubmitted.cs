namespace EventDrivenWebApplication.Infrastructure.Messaging.Contracts;

public class OrderSubmitted
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}