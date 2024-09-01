using EventDrivenWebApplication.Domain.Interfaces;
using EventDrivenWebApplication.Infrastructure.Messaging.Contracts;
using MassTransit;

namespace EventDrivenWebApplication.API.Consumers;

public class ProductCreatedConsumer : IConsumer<ProductCreatedMessage>
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductCreatedConsumer> _logger;

    public ProductCreatedConsumer(IProductService productService, ILogger<ProductCreatedConsumer> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductCreatedMessage> context)
    {
        var message = context.Message;

        _logger.LogInformation("Received ProductCreatedMessage: {ProductId}, {Name}", message.ProductId, message.Name);

        // Perform any necessary processing with the message
        // For example, updating local cache or triggering other processes

        // Example: Log the receipt and completion
        _logger.LogInformation("Processed ProductCreatedMessage for ProductId: {ProductId}", message.ProductId);

        await Task.CompletedTask;
    }
}