using EventDrivenWebApplication.Domain.Entities;

namespace EventDrivenWebApplication.Domain.Interfaces;

/// <summary>
/// Interface for customer service operations such as registration, retrieval, update, and deletion.
/// </summary>
public interface ICustomerService
{
    /// <summary>
    /// Registers a new customer.
    /// </summary>
    /// <param name="customer">The customer entity to be registered.</param>
    /// <returns>The registered customer object.</returns>
    Task<Customer?> RegisterCustomerAsync(Customer customer);

    /// <summary>
    /// Retrieves a customer by their unique customer ID.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer.</param>
    /// <returns>The customer entity if found, otherwise null.</returns>
    Task<Customer?> GetCustomerByIdAsync(Guid customerId);

    /// <summary>
    /// Updates the details of an existing customer.
    /// </summary>
    /// <param name="customer">The customer entity with updated information.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateCustomerAsync(Customer customer);

    /// <summary>
    /// Deletes a customer based on their customer ID.
    /// </summary>
    /// <param name="customerId">The unique identifier of the customer to be deleted.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteCustomerAsync(Guid customerId);
}
