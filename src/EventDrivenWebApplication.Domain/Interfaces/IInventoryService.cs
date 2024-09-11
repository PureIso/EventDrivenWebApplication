using EventDrivenWebApplication.Domain.Entities;

public interface IInventoryService
{
    Task AddProductToInventoryIfNotExistsAsync(Guid productId, string productName);
    Task UpdateInventoryAsync(Guid productId, int quantityChange, bool isChecked, DateTime? checkedAt);
    Task<IEnumerable<InventoryItem>> GetInventoryItemsAsync();
    Task<InventoryItem?> GetInventoryItemAsync(Guid productId);
    Task MarkItemCheckedAsync(Guid productId);
    Task RecordInventoryCheckAsync(Guid productId, bool isAvailable, int totalUniqueItems, int totalQuantity);
}