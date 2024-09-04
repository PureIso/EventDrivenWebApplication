namespace EventDrivenWebApplication.Infrastructure.Messaging.Contracts;

public class OrderAccepted
{
    public Guid OrderId { get; set; }
}