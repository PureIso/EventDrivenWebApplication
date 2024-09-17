using EventDrivenWebApplication.Domain.Entities;

namespace EventDrivenWebApplication.Domain.Interfaces;

/// <summary>
/// Interface for managing inventory items.
/// </summary>
public interface IInventoryService
{
    /// <summary>
    /// Adds a new inventory item.
    /// </summary>
    /// <param name="inventoryItem">The inventory item to add.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    Task AddInventoryItemAsync(InventoryItem inventoryItem, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the quality check status and last checked time for a given inventory item.
    /// </summary>
    /// <param name="inventoryItemId">The ID of the inventory item to update.</param>
    /// <param name="isChecked">Indicates if the item has been quality checked.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    Task UpdateInventoryAsync(int inventoryItemId, bool isChecked, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all inventory items.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A list of inventory items.</returns>
    Task<IEnumerable<InventoryItem>> GetInventoryItemsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a single inventory item by its ID.
    /// </summary>
    /// <param name="inventoryItemId">The ID of the inventory item to retrieve.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The matching inventory item, or null if not found.</returns>
    Task<InventoryItem?> GetInventoryItemUsingProductIdAsync(int inventoryItemId, CancellationToken cancellationToken);

    /// <summary>
    /// Marks an inventory item as checked.
    /// </summary>
    /// <param name="inventoryItemId">The ID of the inventory item to mark as checked.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    Task MarkItemCheckedAsync(int inventoryItemId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an inventory item by its ID.
    /// </summary>
    /// <param name="inventoryItemId">The ID of the inventory item to delete.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    Task DeleteInventoryItemAsync(int inventoryItemId, CancellationToken cancellationToken);
}
