using System.ComponentModel.DataAnnotations;

namespace EventDrivenWebApplication.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public Guid? CorrelationId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = default!;

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0.0, double.MaxValue)]
    public decimal Price { get; set; }
    public DateTime DateTimeCreated { get; set; } = DateTime.UtcNow;
    public DateTime DateTimeLastModified { get; set; } = DateTime.UtcNow;
}