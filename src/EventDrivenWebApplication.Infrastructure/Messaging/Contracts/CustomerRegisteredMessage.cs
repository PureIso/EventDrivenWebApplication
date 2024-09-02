namespace EventDrivenWebApplication.Infrastructure.Messaging.Contracts;

public class CustomerRegisteredMessage
{
    public Guid CustomerId { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public DateTime RegisteredAt { get; set; }
}