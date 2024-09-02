using EventDrivenWebApplication.Domain.Entities;

namespace EventDrivenWebApplication.Domain.Interfaces;

public interface ICustomerService
{
    Task<Customer> RegisterCustomerAsync(Customer customer);
    Task<Customer> GetCustomerByIdAsync(Guid customerId);
    Task UpdateCustomerAsync(Customer customer);
    Task DeleteCustomerAsync(Guid customerId);
}