namespace EventDrivenWebApplication.Infrastructure.Messaging.Contracts;

/// <summary>
/// Represents a message indicating that a customer has been registered.
/// </summary>
public class CustomerRegisteredMessage
{
    /// <summary>
    /// Gets or sets the unique identifier for the registered customer.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the first name of the registered customer.
    /// </summary>
    public string FirstName { get; set; } = default!;

    /// <summary>
    /// Gets or sets the last name of the registered customer.
    /// </summary>
    public string LastName { get; set; } = default!;

    /// <summary>
    /// Gets or sets the email address of the registered customer.
    /// </summary>
    public string Email { get; set; } = default!;

    /// <summary>
    /// Gets or sets the date and time when the customer was registered.
    /// </summary>
    public DateTime RegisteredAt { get; set; }
}