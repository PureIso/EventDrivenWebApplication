using MassTransit;
using EventDrivenWebApplication.Domain.Interfaces;
using EventDrivenWebApplication.Domain.Entities;
using EventDrivenWebApplication.Domain.Messages;

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
            //// Send to NiFi via HTTP
            //using (HttpClient httpClient = new HttpClient())
            //{
            //    HttpResponseMessage response = await httpClient.PostAsJsonAsync("http://localhost:8082/process-data", message);
            //    response.EnsureSuccessStatusCode();
            //}

            _logger.LogInformation("Received ProductCreatedMessage: {ProductId}, {Name}, {Quantity}", message.ProductId, message.Name, message.Quantity);

            // Retry logic to wait for the correct saga state
            bool isSagaReady = await RetryUntilSagaStateAsync(
                message.CorrelationId,
                "Initial",
                "WaitingForInventoryCheckRequest",
                maxRetries: 5,
                initialRetryDelay: TimeSpan.FromMilliseconds(200),
                cancellationToken: cancellationToken
            );

            if (!isSagaReady)
            {
                _logger.LogWarning("Saga did not transition to 'Initial' state after maximum retries. Exiting.");
                return;
            }

            // Proceed with publishing the InventoryCheckRequested message
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ProductCreatedMessage: {ProductId}", message.ProductId);
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
