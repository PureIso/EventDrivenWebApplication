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
            // Mark the inventory item as checked
            await _inventoryService.MarkItemCheckedAsync(message.OrderId);

            _logger.LogInformation($"Inventory check completed for OrderId: {message.OrderId}, IsAvailable: {message.IsAvailable}");

            if (message.IsAvailable)
            {
                // Update inventory quantity if needed based on your business logic
                // This may involve more actions or calling other services.
            }
            else
            {
                // Handle the case where the item is not available.
                // This might include notifying the user or handling order rejection.
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing InventoryCheckCompleted message.");
            throw; // Optional: rethrow exception or handle it as needed
        }
    }
}