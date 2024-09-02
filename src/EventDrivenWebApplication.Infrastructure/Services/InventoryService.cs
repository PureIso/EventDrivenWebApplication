using EventDrivenWebApplication.Domain.Entities;
using EventDrivenWebApplication.Domain.Interfaces;
using EventDrivenWebApplication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventDrivenWebApplication.Infrastructure.Services;

public class InventoryService : IInventoryService
{
    private readonly InventoryDbContext _dbContext;

    public InventoryService(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddProductToInventoryIfNotExistsAsync(Guid productId, string productName)
    {
        InventoryItem? existingProduct = await _dbContext.InventoryItems
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (existingProduct == null)
        {
            InventoryItem newInventoryItem = new InventoryItem
            {
                ProductId = productId,
                Name = productName,
                Quantity = 0
            };

            _dbContext.InventoryItems.Add(newInventoryItem);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<InventoryItem>> GetInventoryItemsAsync()
    {
        return await _dbContext.InventoryItems.ToListAsync();
    }
}