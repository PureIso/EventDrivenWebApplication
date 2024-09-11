using EventDrivenWebApplication.Domain.Entities;
using MassTransit;
using EventDrivenWebApplication.Infrastructure.Messaging.Contracts;
using EventDrivenWebApplication.Domain.Interfaces;

namespace EventDrivenWebApplication.API.Consumers;

public class InventoryCheckRequestedConsumer : IConsumer<InventoryCheckRequested>
{
    private readonly IInventoryService _inventoryService;
    private readonly IProductService _productService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<InventoryCheckRequestedConsumer> _logger;

    public InventoryCheckRequestedConsumer(
        IInventoryService inventoryService,
        IProductService productService,     // Injected ProductService
        IPublishEndpoint publishEndpoint,
        ILogger<InventoryCheckRequestedConsumer> logger)
    {
        _inventoryService = inventoryService;
        _productService = productService;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<InventoryCheckRequested> context)
    {
        InventoryCheckRequested message = context.Message;

        try
        {
            // Fetch product details from ProductService
            Product? product = await _productService.GetProductByIdAsync(message.ProductId);
            if (product == null)
            {
                _logger.LogWarning($"Product with ProductId: {message.ProductId} does not exist in the product database.");
                return;
            }

            // Fetch the item from inventory
            InventoryItem? inventoryItem = await _inventoryService.GetInventoryItemAsync(message.ProductId);
            bool isAvailable = false;
            if (inventoryItem == null)
            {
                // Product does not exist in inventory, add it with the fetched product details
                _logger.LogInformation($"ProductId: {message.ProductId} does not exist in inventory. Adding product to inventory.");
                await _inventoryService.AddProductToInventoryIfNotExistsAsync(product.ProductId, product.Name);
                inventoryItem = await _inventoryService.GetInventoryItemAsync(product.ProductId);
            }
            if (inventoryItem != null && inventoryItem.Quantity > 0)
            {
                isAvailable = true;
            }
            InventoryCheckCompleted inventoryCheckCompleted = new InventoryCheckCompleted
            {
                OrderId = message.OrderId,
                ProductId = message.ProductId,
                IsAvailable = isAvailable,
                CheckedAt = DateTime.UtcNow
            };
            await _publishEndpoint.Publish(inventoryCheckCompleted);
            _logger.LogInformation($"Published InventoryCheckCompleted for OrderId: {message.OrderId}, ProductId: {message.ProductId}, IsAvailable: {isAvailable}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing InventoryCheckRequested message.");
            throw; // Optional: rethrow exception or handle it as needed
        }
    }
}