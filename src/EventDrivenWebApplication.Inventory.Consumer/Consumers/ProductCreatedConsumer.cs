using EventDrivenWebApplication.Messaging.Contracts;
using MassTransit;

namespace EventDrivenWebApplication.Inventory.Consumer.Consumers;

public class ProductCreatedConsumer : IConsumer<ProductCreated>
{
    private readonly ILogger<ProductCreatedConsumer> _logger;

    public ProductCreatedConsumer(ILogger<ProductCreatedConsumer> logger)
    {
        _logger = logger;
    }
    public Task Consume(ConsumeContext<ProductCreated> context)
    {
        _logger.LogInformation(context.Message.ToString());
        return Task.CompletedTask;
    }
}