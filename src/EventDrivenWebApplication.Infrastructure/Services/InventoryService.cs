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

    public async Task UpdateInventoryAsync(Guid productId, int quantityChange, bool isChecked, DateTime? checkedAt)
    {
        InventoryItem? inventoryItem = await _dbContext.InventoryItems
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (inventoryItem != null)
        {
            inventoryItem.Quantity += quantityChange;
            inventoryItem.IsChecked = isChecked;
            inventoryItem.LastCheckedAt = checkedAt;

            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<InventoryItem>> GetInventoryItemsAsync()
    {
        return await _dbContext.InventoryItems.ToListAsync();
    }

    public async Task<InventoryItem?> GetInventoryItemAsync(Guid productId)
    {
        InventoryItem? inventoryItem = await _dbContext.InventoryItems
            .FirstOrDefaultAsync(p => p.ProductId == productId);
        return inventoryItem;
    }

    public async Task MarkItemCheckedAsync(Guid productId)
    {
        InventoryItem? inventoryItem = await _dbContext.InventoryItems
            .FirstOrDefaultAsync(p => p.ProductId == productId);
        if (inventoryItem != null)
        {
            inventoryItem.IsChecked = true;
            inventoryItem.LastCheckedAt = DateTime.UtcNow;
        }
        await _dbContext.SaveChangesAsync();
    }

    public async Task RecordInventoryCheckAsync(Guid productId, bool isAvailable, int totalUniqueItems, int totalQuantity)
    {
        InventoryCheckLog checkLog = new InventoryCheckLog
        {
            ProductId = productId,
            TotalUniqueItems = totalUniqueItems,
            TotalQuantity = totalQuantity,
            IsAvailable = isAvailable,
            CheckedAt = DateTime.UtcNow
        };
        _dbContext.InventoryCheckLogs.Add(checkLog);
        await _dbContext.SaveChangesAsync();
    }
}