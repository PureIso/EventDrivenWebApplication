using Asp.Versioning;
using EventDrivenWebApplication.Domain.Entities;
using EventDrivenWebApplication.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventDrivenWebApplication.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomerController>? _logger;

    public CustomerController(ICustomerService customerService, ILogger<CustomerController>? logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> RegisterCustomer([FromBody] Customer customer)
    {
        Customer? registeredCustomer = await _customerService.RegisterCustomerAsync(customer);
        return CreatedAtAction(nameof(GetCustomerById), new { id = registeredCustomer.CustomerId }, registeredCustomer);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCustomerById(Guid id)
    {
        Customer? customer = await _customerService.GetCustomerByIdAsync(id);
        return Ok(customer);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] Domain.Entities.Customer customer)
    {
        if (id != customer.CustomerId)
            return BadRequest("Customer ID mismatch.");

        await _customerService.UpdateCustomerAsync(customer);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCustomer(Guid id)
    {
        await _customerService.DeleteCustomerAsync(id);
        return NoContent();
    }
}