using EventDrivenWebApplication.Domain.Entities;

namespace EventDrivenWebApplication.Domain.Interfaces;

/// <summary>
/// Interface for managing products.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Creates a new product and publishes a product created event.
    /// </summary>
    /// <param name="product">The product to create.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The created product.</returns>
    Task<Product> CreateProductAsync(Product product, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a product by its ID.
    /// </summary>
    /// <param name="productId">The ID of the product to retrieve.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The product, or null if not found.</returns>
    Task<Product?> GetProductByIdAsync(int productId, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="product">The product to update.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    Task UpdateProductAsync(Product product, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a product by its ID.
    /// </summary>
    /// <param name="productId">The ID of the product to delete.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    Task DeleteProductAsync(int productId, CancellationToken cancellationToken);
}