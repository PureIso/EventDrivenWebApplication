namespace EventDrivenWebApplication.Domain.Entities;

/// <summary>
/// Represents an inventory item.
/// </summary>
public class InventoryItem
{
    /// <summary>
    /// Gets or sets the unique identifier for the inventory item.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the product ID associated with the inventory item.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the item has passed the quality validation.
    /// </summary>
    public bool QualityValidated { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the item was last quality checked.
    /// </summary>
    public DateTime? DateTimeLastQualityChecked { get; set; }
}