namespace EventDrivenWebApplication.Domain.Events
{
    public class CustomerRegisteredEvent
    {
        public Guid CustomerId { get; set; }
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    }
}