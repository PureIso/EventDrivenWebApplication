using EventDrivenWebApplication.Infrastructure.Messaging.Contracts;
using MassTransit;

namespace EventDrivenWebApplication.API.Consumers;

public class ProductCreatedConsumer : IConsumer<ProductCreatedMessage>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ProductCreatedConsumer> _logger;

    public ProductCreatedConsumer(IPublishEndpoint publishEndpoint, ILogger<ProductCreatedConsumer> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductCreatedMessage> context)
    {
        ProductCreatedMessage message = context.Message;
        _logger.LogInformation("Received ProductCreatedMessage: {ProductId}, {Name}, {Quantity}", message.ProductId, message.Name, message.Quantity);
        // Request an inventory check
        InventoryCheckRequested inventoryCheckRequested = new InventoryCheckRequested
        {
            OrderId = Guid.NewGuid(),
            ProductId = message.ProductId,
            Quantity = message.Quantity
        };
        await _publishEndpoint.Publish(inventoryCheckRequested);
        _logger.LogInformation("Published InventoryCheckRequested for ProductId: {ProductId}, Quantity: {Quantity}", message.ProductId, message.Quantity);
    }
}