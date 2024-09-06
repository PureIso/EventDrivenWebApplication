using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using EventDrivenWebApplication.Infrastructure.Messaging.Contracts;
using EventDrivenWebApplication.Domain.Interfaces;

namespace EventDrivenWebApplication.API.Consumers
{
    public class InventoryCheckRequestedConsumer : IConsumer<InventoryCheckRequested>
    {
        private readonly IInventoryService _inventoryService;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<InventoryCheckRequestedConsumer> _logger;

        public InventoryCheckRequestedConsumer(IInventoryService inventoryService, IPublishEndpoint publishEndpoint, ILogger<InventoryCheckRequestedConsumer> logger)
        {
            _inventoryService = inventoryService;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<InventoryCheckRequested> context)
        {
            var message = context.Message;

            try
            {
                // Fetch the item from inventory
                var inventoryItem = await _inventoryService.GetInventoryItemAsync(message.ProductId);

                bool isAvailable = inventoryItem != null && inventoryItem.Quantity >= message.Quantity;

                // Send InventoryCheckCompleted message
                var inventoryCheckCompleted = new InventoryCheckCompleted
                {
                    OrderId = message.OrderId,
                    IsAvailable = isAvailable
                };

                await _publishEndpoint.Publish(inventoryCheckCompleted);

                _logger.LogInformation($"Inventory check requested for OrderId: {message.OrderId}, ProductId: {message.ProductId}, Quantity: {message.Quantity}, IsAvailable: {isAvailable}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing InventoryCheckRequested message.");
                throw; // Optional: rethrow exception or handle it as needed
            }
        }
    }
}
