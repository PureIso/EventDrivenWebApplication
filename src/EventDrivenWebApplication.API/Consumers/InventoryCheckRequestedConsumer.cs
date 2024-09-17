using EventDrivenWebApplication.Domain.Entities;
using EventDrivenWebApplication.Domain.Interfaces;
using EventDrivenWebApplication.Infrastructure.Messaging.Contracts;
using MassTransit;

namespace EventDrivenWebApplication.API.Consumers;

/// <summary>
/// Consumer for handling InventoryCheckRequested messages.
/// </summary>
public class InventoryCheckRequestedConsumer : IConsumer<InventoryCheckRequested>
{
    private readonly IInventoryService _inventoryService;
    private readonly IProductService _productService;
    private readonly IOrderProcessStateService _orderProcessStateService;
    private readonly ILogger<InventoryCheckRequestedConsumer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryCheckRequestedConsumer"/> class.
    /// </summary>
    /// <param name="inventoryService">The inventory service to be used by the consumer.</param>
    /// <param name="productService">The product service to be used by the consumer.</param>
    /// <param name="orderProcessStateService">The service to interact with saga states.</param>
    /// <param name="logger">The logger to be used by the consumer.</param>
    public InventoryCheckRequestedConsumer(
        IInventoryService inventoryService,
        IProductService productService,
        IOrderProcessStateService orderProcessStateService,
        ILogger<InventoryCheckRequestedConsumer> logger)
    {
        _inventoryService = inventoryService;
        _productService = productService;
        _orderProcessStateService = orderProcessStateService;
        _logger = logger;
    }

    /// <summary>
    /// Consumes an <see cref="InventoryCheckRequested"/> message and processes it.
    /// </summary>
    /// <param name="context">The context of the consumed message.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task Consume(ConsumeContext<InventoryCheckRequested> context)
    {
        InventoryCheckRequested message = context.Message;
        CancellationToken cancellationToken = context.CancellationToken;

        try
        {
            int retryCount = 0;
            int maxRetries = 5;
            TimeSpan retryDelay = TimeSpan.FromSeconds(5);

            // Retry logic: wait for the saga to transition to the 'InventoryCheckRequestedState'
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
                if (orderProcessState.PreviousState == "WaitingForInventoryCheckRequest" &&
                    orderProcessState.CurrentState == "InventoryCheckRequestedState")
                {
                    _logger.LogInformation("Saga reached the 'WaitingForInventoryCheckRequest' state. Proceeding.");
                    break;
                }

                retryCount++;
                _logger.LogInformation("Waiting for saga to reach 'WaitingForInventoryCheckRequest' state. Retry attempt: {RetryCount}", retryCount);

                // Wait before retrying
                await Task.Delay(retryDelay, cancellationToken);
            }

            // If the saga is still not in the correct state, log and exit
            if (retryCount == maxRetries)
            {
                _logger.LogWarning("Saga did not transition to 'WaitingForInventoryCheckRequest' state after {MaxRetries} retries. Exiting.", maxRetries);
                return;
            }

            // Fetch the product
            Product? product = await _productService.GetProductByIdAsync(message.ProductId, cancellationToken);
            if (product == null)
            {
                _logger.LogWarning("Product with ProductId: {ProductId} does not exist in the product database.", message.ProductId);
                return;
            }

            // Fetch or create the inventory item
            InventoryItem? inventoryItem = await _inventoryService.GetInventoryItemUsingProductIdAsync(message.ProductId, cancellationToken);
            if (inventoryItem == null)
            {
                _logger.LogInformation("ProductId: {ProductId} does not exist in inventory. Adding product to inventory.", message.ProductId);
                inventoryItem = new InventoryItem
                {
                    ProductId = product.Id,
                    DateTimeLastQualityChecked = DateTime.UtcNow,
                    QualityValidated = true
                };
                await _inventoryService.AddInventoryItemAsync(inventoryItem, cancellationToken);
                inventoryItem = await _inventoryService.GetInventoryItemUsingProductIdAsync(inventoryItem.ProductId, cancellationToken);
            }

            if (inventoryItem == null)
            {
                _logger.LogWarning("Inventory with ProductId: {ProductId} could not be created or fetched.", message.ProductId);
                return;
            }

            // Create and publish InventoryCheckCompleted event
            InventoryCheckCompleted inventoryCheckCompleted = new InventoryCheckCompleted
            {
                CorrelationId = message.CorrelationId,
                InventoryItemId = inventoryItem.Id,
                ProductId = inventoryItem.ProductId,
                IsQualityGood = inventoryItem.QualityValidated && product.Quantity > 0,
                DateTimeInventoryCompleted = DateTime.UtcNow
            };

            // Publish the InventoryCheckCompleted event using context.Publish()
            await context.Publish(inventoryCheckCompleted, cancellationToken);
            _logger.LogInformation("Published InventoryCheckCompleted for ProductId: {ProductId}", message.ProductId);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Operation was canceled during the processing of InventoryCheckRequested message: {ProductId}", message.ProductId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing InventoryCheckRequested message: {ProductId}", message.ProductId);
            throw;
        }
    }
}
