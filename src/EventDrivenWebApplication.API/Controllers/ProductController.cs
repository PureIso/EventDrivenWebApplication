using EventDrivenWebApplication.Domain.Entities;
using EventDrivenWebApplication.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventDrivenWebApplication.API.Controllers
{
    /// <summary>
    /// Handles operations related to products, including creation, retrieval, update, and deletion.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductController"/> class.
        /// </summary>
        /// <param name="productService">The product service to be used by the controller.</param>
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Creates a new product.
        /// </summary>
        /// <param name="product">The product to create.</param>
        /// <returns>A <see cref="Task{IActionResult}"/> representing the asynchronous operation.</returns>
        /// <response code="201">Returns the created product.</response>
        /// <response code="400">If the product is null.</response>
        /// <response code="500">If the operation is canceled.</response>
        [HttpPost]
        public async Task<IActionResult> CreateProductAsync([FromBody] Product product)
        {
            if (product == null)
            {
                return BadRequest("Product cannot be null.");
            }

            try
            {
                // Use HttpContext.RequestAborted to get the cancellation token
                CancellationToken cancellationToken = HttpContext.RequestAborted;
                Product createdProduct = await _productService.CreateProductAsync(product, cancellationToken);
                return CreatedAtRoute("GetProductById", new { id = createdProduct.Id }, createdProduct);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Operation was canceled.");
            }
        }

        /// <summary>
        /// Retrieves a product by its ID.
        /// </summary>
        /// <param name="id">The ID of the product to retrieve.</param>
        /// <returns>A <see cref="Task{IActionResult}"/> representing the asynchronous operation.</returns>
        /// <response code="200">Returns the product with the specified ID.</response>
        /// <response code="404">If the product is not found.</response>
        /// <response code="500">If the operation is canceled.</response>
        [HttpGet("{id}", Name = "GetProductById")]
        public async Task<IActionResult> GetProductByIdAsync(int id)
        {
            try
            {
                // Use HttpContext.RequestAborted to get the cancellation token
                CancellationToken cancellationToken = HttpContext.RequestAborted;
                Product? product = await _productService.GetProductByIdAsync(id, cancellationToken);
                return Ok(product);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Product not found.");
            }
            catch (OperationCanceledException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Operation was canceled.");
            }
        }

        /// <summary>
        /// Updates an existing product.
        /// </summary>
        /// <param name="id">The ID of the product to update.</param>
        /// <param name="product">The updated product details.</param>
        /// <returns>A <see cref="Task{IActionResult}"/> representing the asynchronous operation.</returns>
        /// <response code="204">If the product is successfully updated.</response>
        /// <response code="400">If the product ID in the URL does not match the ID in the body.</response>
        /// <response code="404">If the product is not found.</response>
        /// <response code="500">If the operation is canceled.</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProductAsync(int id, [FromBody] Product product)
        {
            if (id != product.Id)
            {
                return BadRequest("Product ID mismatch.");
            }

            try
            {
                // Use HttpContext.RequestAborted to get the cancellation token
                CancellationToken cancellationToken = HttpContext.RequestAborted;
                await _productService.UpdateProductAsync(product, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Product not found.");
            }
            catch (OperationCanceledException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Operation was canceled.");
            }
        }

        /// <summary>
        /// Deletes a product by its ID.
        /// </summary>
        /// <param name="id">The ID of the product to delete.</param>
        /// <returns>A <see cref="Task{IActionResult}"/> representing the asynchronous operation.</returns>
        /// <response code="204">If the product is successfully deleted.</response>
        /// <response code="404">If the product is not found.</response>
        /// <response code="500">If the operation is canceled.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductAsync(int id)
        {
            try
            {
                // Use HttpContext.RequestAborted to get the cancellation token
                CancellationToken cancellationToken = HttpContext.RequestAborted;
                await _productService.DeleteProductAsync(id, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Product not found.");
            }
            catch (OperationCanceledException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Operation was canceled.");
            }
        }
    }
}
