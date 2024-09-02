using EventDrivenWebApplication.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace EventDrivenWebApplication.Domain.Entities;

public class Product : IProductBase
{
    public int Id { get; set; }

    public Guid ProductId { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0.0, double.MaxValue)]
    public decimal Price { get; set; }
}