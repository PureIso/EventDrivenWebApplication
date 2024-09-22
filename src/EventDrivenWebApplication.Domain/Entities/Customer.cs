using System.ComponentModel.DataAnnotations;

namespace EventDrivenWebApplication.Domain.Entities;

/// <summary>
/// Represents a customer entity.
/// </summary>
public class Customer
{
    /// <summary>
    /// Gets or sets the unique identifier for the customer in the database.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique GUID for the customer.
    /// </summary>
    public Guid CustomerId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the first name of the customer.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = default!;

    /// <summary>
    /// Gets or sets the last name of the customer.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = default!;

    /// <summary>
    /// Gets or sets the email address of the customer.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    /// <summary>
    /// Gets or sets the optional phone number of the customer.
    /// </summary>
    [Phone]
    public string? PhoneNumber { get; set; }
}
