using EventDrivenWebApplication.Domain.Entities;
using EventDrivenWebApplication.Domain.Interfaces;
using EventDrivenWebApplication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventDrivenWebApplication.Infrastructure.Services;

/// <summary>
/// Service for managing inventory items.
/// </summary>
public class InventoryService : IInventoryService
{
    private readonly InventoryDbContext _inventoryDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryService"/> class.
    /// </summary>
    /// <param name="inventoryDbContext">The inventory database context.</param>
    public InventoryService(InventoryDbContext inventoryDbContext)
    {
        _inventoryDbContext = inventoryDbContext;
    }

    /// <summary>
    /// Adds a new inventory item.
    /// </summary>
    /// <param name="inventoryItem">The inventory item to add.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    public async Task AddInventoryItemAsync(InventoryItem inventoryItem, CancellationToken cancellationToken)
    {
        _inventoryDbContext.InventoryItems.Add(inventoryItem);
        await _inventoryDbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Updates the quality check status and last checked time for a given inventory item.
    /// </summary>
    /// <param name="inventoryItemId">The ID of the inventory item to update.</param>
    /// <param name="isChecked">Indicates if the item has been quality checked.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    public async Task UpdateInventoryAsync(int inventoryItemId, bool isChecked, CancellationToken cancellationToken)
    {
        InventoryItem? inventoryItem = await _inventoryDbContext.InventoryItems
            .FirstOrDefaultAsync(i => i.Id == inventoryItemId, cancellationToken);

        if (inventoryItem != null)
        {
            inventoryItem.QualityValidated = isChecked;
            inventoryItem.DateTimeLastQualityChecked = DateTime.UtcNow;
            await _inventoryDbContext.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Retrieves all inventory items.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A list of inventory items.</returns>
    public async Task<IEnumerable<InventoryItem>> GetInventoryItemsAsync(CancellationToken cancellationToken)
    {
        return await _inventoryDbContext.InventoryItems.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a single inventory item by its ID.
    /// </summary>
    /// <param name="inventoryItemId">The ID of the inventory item to retrieve.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The matching inventory item, or null if not found.</returns>
    public async Task<InventoryItem?> GetInventoryItemAsync(int inventoryItemId, CancellationToken cancellationToken)
    {
        return await _inventoryDbContext.InventoryItems
            .FirstOrDefaultAsync(i => i.Id == inventoryItemId, cancellationToken);
    }

    /// <summary>
    /// Marks an inventory item as checked.
    /// </summary>
    /// <param name="inventoryItemId">The ID of the inventory item to mark as checked.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    public async Task MarkItemCheckedAsync(int inventoryItemId, CancellationToken cancellationToken)
    {
        InventoryItem? inventoryItem = await _inventoryDbContext.InventoryItems
            .FirstOrDefaultAsync(i => i.Id == inventoryItemId, cancellationToken);

        if (inventoryItem != null)
        {
            inventoryItem.QualityValidated = true;
            inventoryItem.DateTimeLastQualityChecked = DateTime.UtcNow;
            await _inventoryDbContext.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Deletes an inventory item by its ID.
    /// </summary>
    /// <param name="inventoryItemId">The ID of the inventory item to delete.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    public async Task DeleteInventoryItemAsync(int inventoryItemId, CancellationToken cancellationToken)
    {
        InventoryItem? inventoryItem = await _inventoryDbContext.InventoryItems
            .FirstOrDefaultAsync(i => i.Id == inventoryItemId, cancellationToken);

        if (inventoryItem != null)
        {
            _inventoryDbContext.InventoryItems.Remove(inventoryItem);
            await _inventoryDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
