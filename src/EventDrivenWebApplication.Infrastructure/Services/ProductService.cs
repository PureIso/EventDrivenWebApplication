using EventDrivenWebApplication.Domain.Entities;
using EventDrivenWebApplication.Domain.Interfaces;
using EventDrivenWebApplication.Infrastructure.Data;
using EventDrivenWebApplication.Infrastructure.Messaging.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventDrivenWebApplication.Infrastructure.Services;

/// <summary>
/// Service for managing product-related operations.
/// </summary>
public class ProductService : IProductService
{
    private readonly ProductDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductService"/> class.
    /// </summary>
    /// <param name="dbContext">The database context for products.</param>
    /// <param name="publishEndpoint">The publish endpoint for sending messages.</param>
    public ProductService(ProductDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    /// <inheritdoc/>
    public async Task<Product> CreateProductAsync(Product product, CancellationToken cancellationToken)
    {
        product.CorrelationId = Guid.NewGuid();
        product.Id = 0;
        // Add product to the database
        await _dbContext.Products.AddAsync(product, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        // Publish product created event
        ProductCreatedMessage productCreatedMessage = new ProductCreatedMessage
        {
            CorrelationId = product.CorrelationId.Value,
            ProductId = product.Id,
            Name = product.Name,
            Quantity = product.Quantity,
            Price = product.Price,
            DateTimeCreated = DateTime.UtcNow
        };

        await _publishEndpoint.Publish(productCreatedMessage, cancellationToken);
        return product;
    }

    /// <inheritdoc/>
    public async Task<Product?> GetProductByIdAsync(int productId, CancellationToken cancellationToken)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task UpdateProductAsync(Product product, CancellationToken cancellationToken)
    {
        Product? existingProduct = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == product.Id, cancellationToken);
        if (existingProduct == null)
            return;
        existingProduct.Name = product.Name;
        existingProduct.Quantity = product.Quantity;
        existingProduct.Price = product.Price;
        existingProduct.DateTimeLastModified = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task DeleteProductAsync(int productId, CancellationToken cancellationToken)
    {
        Product? product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product == null)
            return;
        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}