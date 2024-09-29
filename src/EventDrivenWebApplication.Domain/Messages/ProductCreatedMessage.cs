namespace EventDrivenWebApplication.Domain.Messages;

/// <summary>
/// Represents the event that indicates the creation of a new product.
/// </summary>
public class ProductCreatedMessage
{
    /// <summary>
    /// Gets or sets the correlation ID used by the state machine.
    /// </summary>
    public Guid CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the newly created product.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the quantity of the product created.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the price of the product.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the product was created. Defaults to the current UTC time.
    /// </summary>
    public DateTime DateTimeCreated { get; set; } = DateTime.UtcNow;
}