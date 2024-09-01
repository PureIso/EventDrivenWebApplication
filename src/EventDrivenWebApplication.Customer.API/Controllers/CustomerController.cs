using EventDrivenWebApplication.Domain.Entities;
using EventDrivenWebApplication.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventDrivenWebApplication.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpPost]
    public async Task<IActionResult> RegisterCustomer([FromBody] Customer customer)
    {
        var registeredCustomer = await _customerService.RegisterCustomerAsync(customer);
        return CreatedAtAction(nameof(GetCustomerById), new { id = registeredCustomer.CustomerId }, registeredCustomer);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCustomerById(Guid id)
    {
        var customer = await _customerService.GetCustomerByIdAsync(id);
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