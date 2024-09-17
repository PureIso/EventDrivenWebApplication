using EventDrivenWebApplication.Infrastructure.Messaging.Contracts;
using MassTransit;

namespace EventDrivenWebApplication.Infrastructure.Messaging;

public class OrderProcessingStartedConsumer : IConsumer<ProductCreatedMessage>
{
    public async Task Consume(ConsumeContext<ProductCreatedMessage> context)
    {
        // Process the event
        int orderId = context.Message.ProductId;
        // Perform actions such as sending notifications or updating other systems
    }
}