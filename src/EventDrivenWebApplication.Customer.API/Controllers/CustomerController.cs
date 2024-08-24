using EventDrivenWebApplication.Customer.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventDrivenWebApplication.Customer.API.Controllers;

[ApiController]
[Route("[controller]")]
public class CustomerController : ControllerBase
{
    private readonly CustomerDBContext _dbContext;
    public CustomerController(CustomerDBContext dBContext)
    {
        _dbContext = dBContext;
    }

    [HttpGet]
    [Route("/customers")]
    public async Task<ActionResult<IEnumerable<Data.Customer>>> GetCustomers()
    {
        return await _dbContext.Customer.ToListAsync();
    }

    [HttpPost]
    public async Task PostCustomer(Data.Customer customer)
    {
        _dbContext.Customer.Add(customer);
        await _dbContext.SaveChangesAsync();
    }
}