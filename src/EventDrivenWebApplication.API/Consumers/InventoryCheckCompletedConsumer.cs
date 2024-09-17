using EventDrivenWebApplication.Domain.Entities;
using EventDrivenWebApplication.Domain.Interfaces;
using MassTransit;
using EventDrivenWebApplication.Infrastructure.Messaging.Contracts;

namespace EventDrivenWebApplication.API.Consumers;

/// <summary>
/// Consumer for handling InventoryCheckCompleted messages.
/// </summary>
public class InventoryCheckCompletedConsumer : IConsumer<InventoryCheckCompleted>
{
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<InventoryCheckCompletedConsumer> _logger;
    private readonly IOrderProcessStateService _orderProcessStateService;

    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryCheckCompletedConsumer"/> class.
    /// </summary>
    /// <param name="inventoryService">The inventory service to be used by the consumer.</param>
    /// <param name="logger">The logger to be used for logging information and errors.</param>
    /// <param name="orderProcessStateService">The service to interact with saga states.</param>
    public InventoryCheckCompletedConsumer(IInventoryService inventoryService, ILogger<InventoryCheckCompletedConsumer> logger, IOrderProcessStateService orderProcessStateService)
    {
        _inventoryService = inventoryService;
        _logger = logger;
        _orderProcessStateService = orderProcessStateService;
    }

    /// <summary>
    /// Consumes an <see cref="InventoryCheckCompleted"/> message and updates the inventory item status.
    /// </summary>
    /// <param name="context">The consume context containing the message to process.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Consume(ConsumeContext<InventoryCheckCompleted> context)
    {
        InventoryCheckCompleted message = context.Message;
        CancellationToken cancellationToken = context.CancellationToken;

        try
        {
            _logger.LogInformation("Inventory Check Completed for InventoryItemID: {InventoryItemId}, ProductID: {ProductId}",
                message.InventoryItemId, message.ProductId);

            int retryCount = 0;
            int maxRetries = 5;
            TimeSpan retryDelay = TimeSpan.FromSeconds(5);

            // Retry logic: wait for the saga to transition to the 'InventoryCheckRequestedState' state
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
                if (orderProcessState.PreviousState == "InventoryCheckRequestedState" &&
                    orderProcessState.CurrentState == "Completed")
                {
                    _logger.LogInformation("Saga reached the 'InventoryCheckRequestedState' state. Proceeding.");
                    break;
                }

                retryCount++;
                _logger.LogInformation("Waiting for saga to reach 'InventoryCheckRequestedState' state. Retry attempt: {RetryCount}", retryCount);

                // Wait before retrying
                await Task.Delay(retryDelay, cancellationToken);
            }

            // If the saga is still not in the correct state, log and exit
            if (retryCount == maxRetries)
            {
                _logger.LogWarning("Saga did not transition to 'InventoryCheckRequestedState' state after {MaxRetries} retries. Exiting.", maxRetries);
                return;
            }

            // Fetch the inventory item based on the InventoryItemId from the message
            InventoryItem? inventoryItem = await _inventoryService.GetInventoryItemUsingProductIdAsync(message.InventoryItemId, cancellationToken);

            if (inventoryItem != null)
            {
                // Mark the inventory item as checked
                await _inventoryService.MarkItemCheckedAsync(inventoryItem.Id, cancellationToken);
                _logger.LogInformation("Marked InventoryItemID: {InventoryItemId} as checked.", inventoryItem.Id);
            }
            else
            {
                _logger.LogWarning("InventoryItemID: {InventoryItemId} not found.", message.InventoryItemId);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Operation was canceled during the processing of InventoryCheckCompleted message for InventoryItemID: {InventoryItemId}", message.InventoryItemId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing InventoryCheckCompleted message for InventoryItemID: {InventoryItemId}", message.InventoryItemId);
            throw;
        }
    }
}