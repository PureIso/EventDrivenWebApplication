namespace EventDrivenWebApplication.Infrastructure.Messaging.Contracts;

/// <summary>
/// Represents the event that requests an inventory check for a product.
/// </summary>
public class InventoryCheckRequested
{
    /// <summary>
    /// Gets or sets the correlation ID used to track the process.
    /// </summary>
    public Guid CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the product to check.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the quantity of the product being checked.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the price of the product.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the product was created.
    /// </summary>
    public DateTime DateTimeProductCreated { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the inventory was checked.
    /// </summary>
    public DateTime DateTimeInventoryChecked { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the inventory check request was created. Defaults to the current UTC time.
    /// </summary>
    public DateTime DateTimeCreated { get; set; } = DateTime.UtcNow;
}