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

            // Retry logic: wait for the correct saga state
            bool isSagaReady = await RetryUntilSagaStateAsync(
                message.CorrelationId,
                "InventoryCheckRequestedState",
                "Completed",
                maxRetries: 5,
                initialRetryDelay: TimeSpan.FromMilliseconds(200),
                cancellationToken: cancellationToken
            );

            if (!isSagaReady)
            {
                _logger.LogWarning("Saga did not transition to 'InventoryCheckRequestedState' after maximum retries. Exiting.");
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing InventoryCheckCompleted message for InventoryItemID: {InventoryItemId}", message.InventoryItemId);
        }
    }

    /// <summary>
    /// Retries the saga state check with incremental delays until it reaches the specified state or max retries.
    /// </summary>
    /// <param name="correlationId">The saga's correlation ID.</param>
    /// <param name="previousState">The previous state to check for.</param>
    /// <param name="currentState">The current state to check for.</param>
    /// <param name="maxRetries">The maximum number of retries.</param>
    /// <param name="initialRetryDelay">The initial delay between retries.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the saga reaches the desired state, otherwise false.</returns>
    private async Task<bool> RetryUntilSagaStateAsync(
        Guid correlationId,
        string previousState,
        string currentState,
        int maxRetries,
        TimeSpan initialRetryDelay,
        CancellationToken cancellationToken)
    {
        int retryCount = 0;
        TimeSpan retryDelay = initialRetryDelay;

        while (retryCount < maxRetries)
        {
            OrderProcessState? orderProcessState =
                await _orderProcessStateService.GetOrderProcessStateAsync(correlationId, cancellationToken);

            if (orderProcessState != null &&
                orderProcessState.PreviousState == previousState &&
                orderProcessState.CurrentState == currentState)
            {
                _logger.LogInformation("Saga reached the '{PreviousState}' state. Proceeding.", previousState);
                return true;
            }

            retryCount++;
            _logger.LogInformation("Waiting for saga to reach '{PreviousState}' state. Retry attempt: {RetryCount}", previousState, retryCount);

            // Incremental retry delay
            await Task.Delay(retryDelay, cancellationToken);
            retryDelay += TimeSpan.FromMilliseconds(200);
        }

        return false;
    }
}