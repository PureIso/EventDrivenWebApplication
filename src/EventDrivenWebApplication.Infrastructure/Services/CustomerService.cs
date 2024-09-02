using EventDrivenWebApplication.Domain.Entities;
using EventDrivenWebApplication.Domain.Interfaces;
using EventDrivenWebApplication.Infrastructure.Data;
using EventDrivenWebApplication.Infrastructure.Messaging.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventDrivenWebApplication.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly CustomerDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public CustomerService(CustomerDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Customer> RegisterCustomerAsync(Customer customer)
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

    public async Task<Customer> GetCustomerByIdAsync(Guid customerId)
    {
        return await _dbContext.Customers
                   .FirstOrDefaultAsync(c => c.CustomerId == customerId)
               ?? throw new KeyNotFoundException("Customer not found.");
    }

    public async Task UpdateCustomerAsync(Customer customer)
    {
        Customer? existingCustomer = await _dbContext.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId);

        if (existingCustomer == null)
            throw new KeyNotFoundException("Customer not found.");

        existingCustomer.FirstName = customer.FirstName;
        existingCustomer.LastName = customer.LastName;
        existingCustomer.Email = customer.Email;
        existingCustomer.PhoneNumber = customer.PhoneNumber;

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteCustomerAsync(Guid customerId)
    {
        Customer? customer = await _dbContext.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);

        if (customer == null)
            throw new KeyNotFoundException("Customer not found.");

        _dbContext.Customers.Remove(customer);
        await _dbContext.SaveChangesAsync();
    }
}