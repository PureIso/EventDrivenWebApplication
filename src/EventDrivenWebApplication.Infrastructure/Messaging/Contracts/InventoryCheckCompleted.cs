namespace EventDrivenWebApplication.Infrastructure.Messaging.Contracts;

/// <summary>
/// Represents the event that indicates the completion of an inventory check.
/// </summary>
public class InventoryCheckCompleted
{
    /// <summary>
    /// Gets or sets the correlation ID used to track the process.
    /// </summary>
    public Guid CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the inventory item checked.
    /// </summary>
    public int InventoryItemId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the product associated with the inventory check.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the product's quality passed the inventory check.
    /// </summary>
    public bool IsQualityGood { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the inventory check was completed. Defaults to the current UTC time.
    /// </summary>
    public DateTime DateTimeInventoryCompleted { get; set; } = DateTime.UtcNow;
}