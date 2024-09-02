using EventDrivenWebApplication.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace EventDrivenWebApplication.Domain.Entities;

public class Customer : ICustomerBase
{
    public int Id { get; set; }

    public Guid CustomerId { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = default!;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = default!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Phone]
    public string? PhoneNumber { get; set; }
}