using EventDrivenWebApplication.Domain.Entities;

namespace EventDrivenWebApplication.Domain.Interfaces
{
    public interface IInventoryService
    {
        Task AddProductToInventoryIfNotExistsAsync(Guid productId, string productName);
        Task<IEnumerable<InventoryItem>> GetInventoryItemsAsync();
    }
}
