using Asp.Versioning;
using EventDrivenWebApplication.Domain.Entities;
using EventDrivenWebApplication.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventDrivenWebApplication.API.Controllers;

/// <summary>
/// Controller for managing customer-related operations.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomerController>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerController"/> class.
    /// </summary>
    /// <param name="customerService">The service for managing customers.</param>
    /// <param name="logger">The logger for the controller.</param>
    public CustomerController(ICustomerService customerService, ILogger<CustomerController>? logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new customer.
    /// </summary>
    /// <param name="customer">The customer details.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the registration.</returns>
    [HttpPost]
    public async Task<IActionResult> RegisterCustomer([FromBody] Customer customer)
    {
        Customer? registeredCustomer = await _customerService.RegisterCustomerAsync(customer);
        return CreatedAtAction(nameof(GetCustomerById), new { id = registeredCustomer.CustomerId }, registeredCustomer);
    }

    /// <summary>
    /// Retrieves a customer by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the customer.</param>
    /// <returns>A task that represents the asynchronous operation, containing the customer details.</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCustomerById(Guid id)
    {
        Customer? customer = await _customerService.GetCustomerByIdAsync(id);
        return Ok(customer);
    }

    /// <summary>
    /// Updates the details of an existing customer.
    /// </summary>
    /// <param name="id">The unique identifier of the customer.</param>
    /// <param name="customer">The updated customer details.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] Domain.Entities.Customer customer)
    {
        if (id != customer.CustomerId)
            return BadRequest("Customer ID mismatch.");

        await _customerService.UpdateCustomerAsync(customer);
        return NoContent();
    }

    /// <summary>
    /// Deletes a customer by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the customer to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCustomer(Guid id)
    {
        await _customerService.DeleteCustomerAsync(id);
        return NoContent();
    }
}
