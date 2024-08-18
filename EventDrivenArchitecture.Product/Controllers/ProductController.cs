using EventDrivenArchitecture.Inventory.Data;
using EventDrivenArchitecture.Messaging.Contracts;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventDrivenArchitecture.Inventory.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductController : ControllerBase
{
    private readonly ProductDBContext _dbContext;
    private readonly IBus _bus;
    private IValidator<Product> _productValidator;

    public ProductController(ProductDBContext dBContext, IBus bus, IValidator<Product> productValidator)
    {
        _dbContext = dBContext;
        _bus = bus;
        // Inject our validator and also a DB context for storing our person object.
        _productValidator = productValidator;
    }

    [HttpGet]
    [Route("/products")]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        return await _dbContext.Product.ToListAsync();
    }
    //https://www.youtube.com/watch?v=4FFYefcx4Bg&ab_channel=NickChapsas
    //https://www.youtube.com/watch?v=HWnoD7ywfoI&list=PL6tu16kXT9PpywOvObZKKN24cHkWAXVAj&index=3&ab_channel=ExecuteAutomation
    //https://masstransit.io/quick-starts/rabbitmq


    [HttpPost]
    public async Task<IActionResult> PostProduct([FromBody] Product product)
    {
        //Validate product
        var validationResult = await _productValidator.ValidateAsync(product);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        _dbContext.Product.Add(product);
        int changeCount = await _dbContext.SaveChangesAsync();
        if (changeCount > 0)
        {
            ProductCreated productCreated = new ProductCreated()
            {
                Id = product.Id,
                ProductId = product.ProductId,
                Name = product.Name,
                Quantity = product.Quantity
            };
            //https://www.youtube.com/watch?v=Zh1ccvTFzs8&ab_channel=IAmTimCorey

            await _bus.Publish(productCreated);
            return Ok(productCreated);
        }
        return NotFound();
    }
    [HttpPut]
    public async Task<ActionResult<Product>> UpdateProduct(Product product)
    {
        _dbContext.Product.Update(product);
        await _dbContext.SaveChangesAsync();

        return product;
    }
}
