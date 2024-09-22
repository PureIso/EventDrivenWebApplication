using System.ComponentModel.DataAnnotations;

namespace EventDrivenWebApplication.Domain.Entities;

/// <summary>
/// Represents a product in the system with relevant properties such as name, quantity, and price.
/// </summary>
public class Product
{
    /// <summary>
    /// Gets or sets the unique identifier for the product.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the correlation ID for tracking related operations.
    /// </summary>
    public Guid? CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the quantity of the product available in stock.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the price of the product.
    /// </summary>
    [Range(0.0, double.MaxValue)]
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the product was created.
    /// </summary>
    public DateTime DateTimeCreated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the product was last modified.
    /// </summary>
    public DateTime DateTimeLastModified { get; set; } = DateTime.UtcNow;
}