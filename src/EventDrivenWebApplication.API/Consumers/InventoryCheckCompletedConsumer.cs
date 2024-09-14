using EventDrivenWebApplication.Domain.Entities;
using EventDrivenWebApplication.Domain.Interfaces;
using MassTransit;
using EventDrivenWebApplication.Infrastructure.Messaging.Contracts;

namespace EventDrivenWebApplication.API.Consumers;

/// <summary>
/// Consumer for handling inventory check completed messages.
/// </summary>
public class InventoryCheckCompletedConsumer : IConsumer<InventoryCheckCompleted>
{
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<InventoryCheckCompletedConsumer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryCheckCompletedConsumer"/> class.
    /// </summary>
    /// <param name="inventoryService">The inventory service to be used by the consumer.</param>
    /// <param name="logger">The logger to be used for logging information and errors.</param>
    public InventoryCheckCompletedConsumer(IInventoryService inventoryService, ILogger<InventoryCheckCompletedConsumer> logger)
    {
        _inventoryService = inventoryService;
        _logger = logger;
    }

    /// <summary>
    /// Consumes an <see cref="InventoryCheckCompleted"/> message and updates the inventory item.
    /// </summary>
    /// <param name="context">The consume context containing the message to process.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Consume(ConsumeContext<InventoryCheckCompleted> context)
    {
        InventoryCheckCompleted message = context.Message;

        try
        {
            _logger.LogInformation($"Inventory Check Completed for InventoryItemID: {message.InventoryItemId}, ProductID: {message.ProductId}");

            InventoryItem? inventoryItem = await _inventoryService.GetInventoryItemAsync(message.InventoryItemId, context.CancellationToken);
            if (inventoryItem != null)
            {
                await _inventoryService.MarkItemCheckedAsync(inventoryItem.Id, context.CancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the InventoryCheckCompleted message.");
            throw;
        }
    }
}