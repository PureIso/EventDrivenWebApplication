using EventDrivenWebApplication.Domain.Entities;
using EventDrivenWebApplication.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventDrivenWebApplication.API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] Product product)
    {
        var createdProduct = await _productService.CreateProductAsync(product);
        return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.ProductId }, createdProduct);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        return Ok(product);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] Product product)
    {
        if (id != product.ProductId)
            return BadRequest("Product ID mismatch.");

        await _productService.UpdateProductAsync(product);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        await _productService.DeleteProductAsync(id);
        return NoContent();
    }
}