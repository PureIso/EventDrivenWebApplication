using EventDrivenWebApplication.Domain.Entities;

namespace EventDrivenWebApplication.Domain.Interfaces;

public interface IProductService
{
    Task<Product> CreateProductAsync(Product product);
    Task<Product?> GetProductByIdAsync(Guid productId);
    Task UpdateProductAsync(Product product);
    Task DeleteProductAsync(Guid productId);
}