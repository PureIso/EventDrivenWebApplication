using EventDrivenWebApplication.Domain.Interfaces;
using EventDrivenWebApplication.Infrastructure.Messaging.Contracts;
using MassTransit;

namespace EventDrivenWebApplication.API.Consumers;

public class ProductCreatedConsumer : IConsumer<ProductCreatedMessage>
{
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<ProductCreatedConsumer> _logger;

    public ProductCreatedConsumer(IInventoryService inventoryService, ILogger<ProductCreatedConsumer> logger)
    {
        _inventoryService = inventoryService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductCreatedMessage> context)
    {
        ProductCreatedMessage message = context.Message;

        _logger.LogInformation("Received ProductCreatedMessage: {ProductId}, {Name}", message.ProductId, message.Name);

        // Check if the product exists in inventory; if not, add it
        await _inventoryService.AddProductToInventoryIfNotExistsAsync(message.ProductId, message.Name);

        _logger.LogInformation("Processed ProductCreatedMessage for ProductId: {ProductId}", message.ProductId);
    }
}
