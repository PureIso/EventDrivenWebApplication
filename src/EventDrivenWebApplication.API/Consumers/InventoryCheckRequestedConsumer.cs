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
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<InventoryCheckRequestedConsumer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryCheckRequestedConsumer"/> class.
    /// </summary>
    /// <param name="inventoryService">The inventory service to be used by the consumer.</param>
    /// <param name="productService">The product service to be used by the consumer.</param>
    /// <param name="publishEndpoint">The publish endpoint for publishing messages.</param>
    /// <param name="logger">The logger to be used by the consumer.</param>
    public InventoryCheckRequestedConsumer(
        IInventoryService inventoryService,
        IProductService productService,
        IPublishEndpoint publishEndpoint,
        ILogger<InventoryCheckRequestedConsumer> logger)
    {
        _inventoryService = inventoryService;
        _productService = productService;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    /// <summary>
    /// Consumes an InventoryCheckRequested message and processes it.
    /// </summary>
    /// <param name="context">The context of the consumed message.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task Consume(ConsumeContext<InventoryCheckRequested> context)
    {
        InventoryCheckRequested message = context.Message;
        CancellationToken cancellationToken = context.CancellationToken;

        try
        {
            // Fetch the product
            Product? product = await _productService.GetProductByIdAsync(message.ProductId, cancellationToken);
            if (product == null)
            {
                _logger.LogWarning($"Product with ProductId: {message.ProductId} does not exist in the product database.");
                return;
            }

            // Fetch or create the inventory item
            InventoryItem? inventoryItem = await _inventoryService.GetInventoryItemAsync(message.ProductId, cancellationToken);
            if (inventoryItem == null)
            {
                _logger.LogInformation($"ProductId: {message.ProductId} does not exist in inventory. Adding product to inventory.");
                inventoryItem = new InventoryItem
                {
                    ProductId = product.Id
                };
                await _inventoryService.AddInventoryItemAsync(inventoryItem, cancellationToken);
                inventoryItem = await _inventoryService.GetInventoryItemAsync(inventoryItem.ProductId, cancellationToken);
            }

            if (inventoryItem == null)
            {
                _logger.LogWarning($"Inventory with ProductId: {message.ProductId} could not be created or fetched.");
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

            await _publishEndpoint.Publish(inventoryCheckCompleted, cancellationToken);
            _logger.LogInformation($"Published InventoryCheckCompleted for ProductId: {message.ProductId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing InventoryCheckRequested message.");
            throw;
        }
    }
}