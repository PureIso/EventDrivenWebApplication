using EventDrivenWebApplication.Domain.Entities;
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
        var existingProduct = await _dbContext.InventoryItems
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (existingProduct == null)
        {
            var newInventoryItem = new InventoryItem
            {
                ProductId = productId,
                Name = productName,
                Quantity = 0
            };

            _dbContext.InventoryItems.Add(newInventoryItem);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task UpdateInventoryAsync(Guid productId, int quantityChange, bool isChecked, DateTime? checkedAt)
    {
        var inventoryItem = await _dbContext.InventoryItems
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (inventoryItem == null)
            throw new KeyNotFoundException("Inventory item not found.");

        inventoryItem.Quantity += quantityChange;
        inventoryItem.IsChecked = isChecked;
        inventoryItem.LastCheckedAt = checkedAt;

        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<InventoryItem>> GetInventoryItemsAsync()
    {
        return await _dbContext.InventoryItems.ToListAsync();
    }

    public async Task<InventoryItem> GetInventoryItemAsync(Guid productId)
    {
        var inventoryItem = await _dbContext.InventoryItems
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (inventoryItem == null)
            throw new KeyNotFoundException("Inventory item not found.");

        return inventoryItem;
    }

    public async Task MarkItemCheckedAsync(Guid productId)
    {
        var inventoryItem = await _dbContext.InventoryItems
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (inventoryItem == null)
            throw new KeyNotFoundException("Inventory item not found.");

        inventoryItem.IsChecked = true;
        inventoryItem.LastCheckedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }
}