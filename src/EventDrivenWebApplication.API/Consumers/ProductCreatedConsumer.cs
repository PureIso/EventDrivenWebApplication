using EventDrivenWebApplication.Infrastructure.Messaging.Contracts;
using MassTransit;
using EventDrivenWebApplication.Domain.Interfaces;
using EventDrivenWebApplication.Domain.Entities;

namespace EventDrivenWebApplication.API.Consumers;

/// <summary>
/// Consumes messages related to product creation and triggers inventory checks.
/// </summary>
public class ProductCreatedConsumer : IConsumer<ProductCreatedMessage>
{
    private readonly ILogger<ProductCreatedConsumer> _logger;
    private readonly IOrderProcessStateService _orderProcessStateService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductCreatedConsumer"/> class.
    /// </summary>
    /// <param name="logger">The logger to be used by the consumer.</param>
    /// <param name="orderProcessStateService">The service to interact with saga states.</param>
    public ProductCreatedConsumer(ILogger<ProductCreatedConsumer> logger, IOrderProcessStateService orderProcessStateService)
    {
        _logger = logger;
        _orderProcessStateService = orderProcessStateService;
    }

    /// <summary>
    /// Consumes a <see cref="ProductCreatedMessage"/> and processes it to trigger an inventory check.
    /// </summary>
    /// <param name="context">The context of the consumed message.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task Consume(ConsumeContext<ProductCreatedMessage> context)
    {
        ProductCreatedMessage message = context.Message;
        CancellationToken cancellationToken = context.CancellationToken;

        try
        {
            _logger.LogInformation("Received ProductCreatedMessage: {ProductId}, {Name}, {Quantity}", message.ProductId, message.Name, message.Quantity);

            int retryCount = 0;
            int maxRetries = 5;
            TimeSpan retryDelay = TimeSpan.FromSeconds(5);

            // Retry logic: wait for the saga to transition to the 'WaitingForInventoryCheckRequest' state
            while (retryCount < maxRetries)
            {
                OrderProcessState? orderProcessState =
                    await _orderProcessStateService.GetOrderProcessStateAsync(message.CorrelationId, cancellationToken);
                if (orderProcessState == null)
                {
                    retryCount++;
                    // Wait before retrying
                    await Task.Delay(retryDelay, cancellationToken);
                    continue;
                }
                if (orderProcessState.PreviousState == "Initial" &&
                    orderProcessState.CurrentState == "WaitingForInventoryCheckRequest")
                {
                    _logger.LogInformation("Saga reached the 'Initial' state. Proceeding.");
                    break;
                }

                retryCount++;
                _logger.LogInformation("Waiting for saga to reach 'Initial' state. Retry attempt: {RetryCount}", retryCount);

                // Wait before retrying
                await Task.Delay(retryDelay, cancellationToken);
            }

            // If the saga is still not in the correct state, log and exit
            if (retryCount == maxRetries)
            {
                _logger.LogWarning("Saga did not transition to 'Initial' state after {MaxRetries} retries. Exiting.", maxRetries);
                return;
            }

            // Now the saga is in the correct state, proceed with publishing the InventoryCheckRequested message
            InventoryCheckRequested inventoryCheckRequested = new InventoryCheckRequested
            {
                CorrelationId = message.CorrelationId,
                ProductId = message.ProductId,
                Quantity = message.Quantity,
                Price = message.Price,
                DateTimeInventoryChecked = DateTime.UtcNow,
                DateTimeProductCreated = message.DateTimeCreated
            };

            await context.Publish(inventoryCheckRequested, cancellationToken);

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
