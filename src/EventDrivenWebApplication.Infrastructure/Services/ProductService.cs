using EventDrivenWebApplication.Domain.Entities;
using EventDrivenWebApplication.Domain.Interfaces;
using EventDrivenWebApplication.Infrastructure.Data;
using EventDrivenWebApplication.Infrastructure.Messaging.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventDrivenWebApplication.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly ProductDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public ProductService(ProductDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        product.ProductId = Guid.NewGuid();

        await _dbContext.Products.AddAsync(product);
        await _dbContext.SaveChangesAsync();

        ProductCreatedMessage productCreatedMessage = new ProductCreatedMessage
        {
            ProductId = product.ProductId,
            Name = product.Name,
            Quantity = product.Quantity,
            Price = product.Price,
            CreatedAt = DateTime.UtcNow
        };

        await _publishEndpoint.Publish(productCreatedMessage);

        return product;
    }

    public async Task<Product?> GetProductByIdAsync(Guid productId)
    {
        return await _dbContext.Products
                   .FirstOrDefaultAsync(p => p.ProductId == productId)
               ?? throw new KeyNotFoundException("Product not found.");
    }

    public async Task UpdateProductAsync(Product product)
    {
        Product? existingProduct = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.ProductId == product.ProductId);

        if (existingProduct == null)
            throw new KeyNotFoundException("Product not found.");

        existingProduct.Name = product.Name;
        existingProduct.Quantity = product.Quantity;
        existingProduct.Price = product.Price;

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteProductAsync(Guid productId)
    {
        Product? product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (product == null)
            throw new KeyNotFoundException("Product not found.");

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync();
    }
}