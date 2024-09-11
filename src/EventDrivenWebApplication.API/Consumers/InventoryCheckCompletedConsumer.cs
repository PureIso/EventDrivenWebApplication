using EventDrivenWebApplication.Domain.Entities;
using MassTransit;
using EventDrivenWebApplication.Infrastructure.Messaging.Contracts;

namespace EventDrivenWebApplication.API.Consumers;

public class InventoryCheckCompletedConsumer : IConsumer<InventoryCheckCompleted>
{
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<InventoryCheckCompletedConsumer> _logger;

    public InventoryCheckCompletedConsumer(IInventoryService inventoryService, ILogger<InventoryCheckCompletedConsumer> logger)
    {
        _inventoryService = inventoryService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<InventoryCheckCompleted> context)
    {
        InventoryCheckCompleted message = context.Message;

        try
        {
            _logger.LogInformation($"Inventory check completed for OrderId: {message.OrderId}, IsAvailable: {message.IsAvailable}");

            // Mark item as checked
            await _inventoryService.MarkItemCheckedAsync(message.ProductId);

            // Fetch inventory items
            IEnumerable<InventoryItem>? inventoryItems = await _inventoryService.GetInventoryItemsAsync();
            IEnumerable<InventoryItem> enumerable = inventoryItems.ToList();
            if (enumerable.Any())
            {
                int uniqueItemsCount = enumerable.Count();
                int totalQuantity = enumerable.Sum(item => item.Quantity);
                await _inventoryService.RecordInventoryCheckAsync(message.ProductId, message.IsAvailable, uniqueItemsCount, totalQuantity);
            }
            else
            {
                _logger.LogWarning("No inventory items found to calculate totals.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing InventoryCheckCompleted message for OrderId: {message.OrderId}");
            throw;
        }
    }
}