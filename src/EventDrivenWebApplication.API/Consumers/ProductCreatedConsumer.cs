using EventDrivenWebApplication.Infrastructure.Messaging.Contracts;
using MassTransit;

namespace EventDrivenWebApplication.API.Consumers;

/// <summary>
/// Consumes messages related to product creation and triggers inventory checks.
/// </summary>
public class ProductCreatedConsumer : IConsumer<ProductCreatedMessage>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ProductCreatedConsumer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductCreatedConsumer"/> class.
    /// </summary>
    /// <param name="publishEndpoint">The publish endpoint for publishing messages.</param>
    /// <param name="logger">The logger to be used by the consumer.</param>
    public ProductCreatedConsumer(IPublishEndpoint publishEndpoint, ILogger<ProductCreatedConsumer> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    /// <summary>
    /// Consumes a <see cref="ProductCreatedMessage"/> and processes it to trigger an inventory check.
    /// </summary>
    /// <param name="context">The context of the consumed message.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    public async Task Consume(ConsumeContext<ProductCreatedMessage> context)
    {
        ProductCreatedMessage message = context.Message;
        CancellationToken cancellationToken = context.CancellationToken;

        try
        {
            _logger.LogInformation("Received ProductCreatedMessage: {ProductId}, {Name}, {Quantity}", message.ProductId, message.Name, message.Quantity);

            // Create the InventoryCheckRequested message
            InventoryCheckRequested inventoryCheckRequested = new InventoryCheckRequested
            {
                CorrelationId = message.CorrelationId,
                ProductId = message.ProductId,
                Quantity = message.Quantity,
                Price = message.Price,
                DateTimeInventoryChecked = DateTime.UtcNow,
                DateTimeProductCreated = message.DateTimeCreated
            };

            // Publish the InventoryCheckRequested message
            await _publishEndpoint.Publish(inventoryCheckRequested, cancellationToken);

            _logger.LogInformation("Published InventoryCheckRequested for ProductId: {ProductId}, Quantity: {Quantity}", message.ProductId, message.Quantity);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Operation was canceled during the processing of ProductCreatedMessage: {ProductId}", message.ProductId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ProductCreatedMessage: {ProductId}", message.ProductId);
            throw;
        }
    }
}