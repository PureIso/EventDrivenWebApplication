using EventDrivenWebApplication.Domain.Entities;
using EventDrivenWebApplication.Domain.Interfaces;
using EventDrivenWebApplication.Infrastructure.Data;
using EventDrivenWebApplication.Infrastructure.Messaging.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventDrivenWebApplication.Infrastructure.Services;

/// <summary>
/// Service for managing customer-related operations such as registration, retrieval, update, and deletion.
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly CustomerDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerService"/> class.
    /// </summary>
    /// <param name="dbContext">The customer database context to interact with.</param>
    /// <param name="publishEndpoint">The messaging endpoint used for publishing events.</param>
    public CustomerService(CustomerDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    /// <summary>
    /// Registers a new customer and publishes a customer registration event.
    /// </summary>
    /// <param name="customer">The customer to register.</param>
    /// <returns>The registered customer object.</returns>
    public async Task<Customer?> RegisterCustomerAsync(Customer customer)
    {
        customer.CustomerId = Guid.NewGuid();

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.SaveChangesAsync();

        CustomerRegisteredMessage? customerRegisteredMessage = new CustomerRegisteredMessage
        {
            CustomerId = customer.CustomerId,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            RegisteredAt = DateTime.UtcNow
        };

        await _publishEndpoint.Publish(customerRegisteredMessage);

        return customer;
    }

    /// <summary>
    /// Retrieves a customer by their unique customer ID.
    /// </summary>
    /// <param name="customerId">The unique customer ID.</param>
    /// <returns>The customer object if found, otherwise null.</returns>
    public async Task<Customer?> GetCustomerByIdAsync(Guid customerId)
    {
        return await _dbContext.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);
    }

    /// <summary>
    /// Updates the details of an existing customer.
    /// </summary>
    /// <param name="customer">The customer object with updated details.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpdateCustomerAsync(Customer customer)
    {
        Customer? existingCustomer = await _dbContext.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId);

        if (existingCustomer != null)
        {
            existingCustomer.FirstName = customer.FirstName;
            existingCustomer.LastName = customer.LastName;
            existingCustomer.Email = customer.Email;
            existingCustomer.PhoneNumber = customer.PhoneNumber;

            await _dbContext.SaveChangesAsync();
        }
        else
        {
            _dbContext.Entry(customer).State = EntityState.Detached;
        }
    }

    /// <summary>
    /// Deletes a customer from the database based on their customer ID.
    /// </summary>
    /// <param name="customerId">The unique customer ID.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteCustomerAsync(Guid customerId)
    {
        Customer? customer = await _dbContext.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);

        if (customer != null)
        {
            _dbContext.Customers.Remove(customer);
            await _dbContext.SaveChangesAsync();
        }
    }
}
