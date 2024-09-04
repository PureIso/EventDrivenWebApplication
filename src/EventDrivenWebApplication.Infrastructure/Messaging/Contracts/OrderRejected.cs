namespace EventDrivenWebApplication.Infrastructure.Messaging.Contracts;

public class OrderRejected
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; }
}