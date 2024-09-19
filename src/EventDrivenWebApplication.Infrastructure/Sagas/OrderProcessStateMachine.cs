using EventDrivenWebApplication.Domain.Entities;
using EventDrivenWebApplication.Infrastructure.Messaging.Contracts;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace EventDrivenWebApplication.Infrastructure.Sagas;

/// <summary>
/// State machine for processing orders, handling events related to product creation and inventory checks.
/// </summary>
public class OrderProcessStateMachine : MassTransitStateMachine<OrderProcessState>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrderProcessStateMachine"/> class.
    /// </summary>
    public OrderProcessStateMachine()
    {
        InstanceState(x => x.CurrentState);

        // Initialize state properties
        WaitingForInventoryCheckRequest = State("WaitingForInventoryCheckRequest");
        InventoryCheckRequestedState = State("InventoryCheckRequestedState");
        Completed = State("Completed");

        // Define events and their correlation by CorrelationId
        Event(() => ProductCreated, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => InventoryCheckRequested, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => InventoryCheckCompleted, x => x.CorrelateById(context => context.Message.CorrelationId));

        // Define the initial state transition from ProductCreated
        Initially(
            When(ProductCreated)
                .ThenAsync(async context =>
                {
                    context.Saga.PreviousState = context.Saga.CurrentState;
                    await ProcessProductCreated(context);
                    await LogStateTransitionAsync(context, "ProductCreated event processed", "Exit");
                })
                .TransitionTo(WaitingForInventoryCheckRequest),

            When(InventoryCheckRequested)
                .Then(context => LogDuplicateOrOutOfOrder(context, "InventoryCheckRequested (out-of-order)")),

            When(InventoryCheckCompleted)
                .Then(context => LogDuplicateOrOutOfOrder(context, "InventoryCheckCompleted (out-of-order)"))
        );

        // Handle events while waiting for inventory check request
        During(WaitingForInventoryCheckRequest,
            When(ProductCreated)
                .Then(context => LogDuplicateOrOutOfOrder(context, "ProductCreated (out-of-order)")),

            When(InventoryCheckRequested)
                .ThenAsync(async context =>
                {
                    context.Saga.PreviousState = context.Saga.CurrentState;
                    await ProcessInventoryCheckRequested(context);
                    await LogStateTransitionAsync(context, "InventoryCheckRequested event processed", "Exit");
                })
                .TransitionTo(InventoryCheckRequestedState),

            When(InventoryCheckCompleted)
                .Then(context => LogDuplicateOrOutOfOrder(context, "InventoryCheckCompleted (out-of-order)"))
        );

        // Handle events while inventory check has been requested
        During(InventoryCheckRequestedState,
            When(ProductCreated)
                .Then(context => LogDuplicateOrOutOfOrder(context, "ProductCreated (out-of-order)")),

            When(InventoryCheckRequested)
                .Then(context => LogDuplicateOrOutOfOrder(context, "InventoryCheckRequested (out-of-order)")),

            When(InventoryCheckCompleted)
                .ThenAsync(async context =>
                {
                    context.Saga.PreviousState = context.Saga.CurrentState;
                    await ProcessInventoryCheckCompleted(context);
                    await LogStateTransitionAsync(context, "InventoryCheckCompleted event processed", "Exit");
                })
                .Finalize()
        );

        // Mark the saga as completed when it reaches the final state
        SetCompletedWhenFinalized();
    }

    /// <summary>
    /// Logs a duplicate or out-of-order event.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="context">The behavior context.</param>
    /// <param name="eventName">The name of the event.</param>
    private void LogDuplicateOrOutOfOrder<TEvent>(BehaviorContext<OrderProcessState, TEvent> context, string eventName)
        where TEvent : class
    {
        string productId = context.Saga.ProductId != 0 ? context.Saga.ProductId.ToString() : "Unknown";
        Log.Warning($"{eventName} is duplicate or out-of-order. ProductID: {productId}, CurrentState: {context.Saga.CurrentState}");
    }

    /// <summary>
    /// Processes the ProductCreated event.
    /// </summary>
    /// <param name="context">The behavior context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ProcessProductCreated(BehaviorContext<OrderProcessState, ProductCreatedMessage> context)
    {
        await LogStateTransitionAsync(context, "ProductCreated event processed", "Entry");

        context.Saga.CorrelationId = context.Message.CorrelationId;
        context.Saga.ProductId = context.Message.ProductId;
        context.Saga.ProductName = context.Message.Name;
        context.Saga.ProductQuantity = context.Message.Quantity;
        context.Saga.Price = context.Message.Price;
        context.Saga.DateTimeProductCreated = context.Message.DateTimeCreated;

        Log.Information($"ProductCreated event processed for ProductId: {context.Message.ProductId}. Transitioning to WaitingForInventoryCheckRequest.");
    }

    /// <summary>
    /// Processes the InventoryCheckRequested event.
    /// </summary>
    /// <param name="context">The behavior context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ProcessInventoryCheckRequested(BehaviorContext<OrderProcessState, InventoryCheckRequested> context)
    {
        await LogStateTransitionAsync(context, "InventoryCheckRequested event processed", "Entry");

        context.Saga.CorrelationId = context.Message.CorrelationId;
        context.Saga.DateTimeInventoryCheckRequested = context.Message.DateTimeCreated;

        Log.Information($"InventoryCheckRequested event processed for ProductID: {context.Message.ProductId}. Transitioning to InventoryCheckRequestedState.");
    }

    /// <summary>
    /// Processes the InventoryCheckCompleted event.
    /// </summary>
    /// <param name="context">The behavior context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ProcessInventoryCheckCompleted(BehaviorContext<OrderProcessState, InventoryCheckCompleted> context)
    {
        await LogStateTransitionAsync(context, "InventoryCheckCompleted event processed", "Entry");

        context.Saga.CorrelationId = context.Message.CorrelationId;
        context.Saga.IsQualityGood = context.Message.IsQualityGood;
        context.Saga.DateTimeInventoryCheckCompleted = context.Message.DateTimeInventoryCompleted;

        Log.Information($"InventoryCheckCompleted event processed for ProductID: {context.Message.ProductId}. Transitioning to Completed.");
    }

    /// <summary>
    /// Logs the state transition asynchronously.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="context">The behavior context.</param>
    /// <param name="description">The description of the transition.</param>
    /// <param name="entryOrExit">Indicates whether it is an entry or exit transition.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task LogStateTransitionAsync<TEvent>(BehaviorContext<OrderProcessState, TEvent> context, string description, string entryOrExit)
        where TEvent : class
    {
        using IServiceScope scope = context.GetPayload<IServiceScopeFactory>().CreateScope();
        OrderSagaDbContext dbContext = scope.ServiceProvider.GetRequiredService<OrderSagaDbContext>();

        OrderProcessStateHistory historyEntry = new OrderProcessStateHistory
        {
            CorrelationId = context.Saga.CorrelationId,
            PreviousState = context.Saga.PreviousState,
            CurrentState = context.Saga.CurrentState,
            TransitionedAt = DateTime.UtcNow,
            Description = $"{description} - {entryOrExit}"
        };

        try
        {
            dbContext.OrderProcessStateHistories.Add(historyEntry);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error saving state transition");
            throw;
        }
    }

    // Define state properties
    private State WaitingForInventoryCheckRequest { get; set; }
    private State InventoryCheckRequestedState { get; set; }
    private State Completed { get; set; }

    // Define event properties
    private Event<ProductCreatedMessage> ProductCreated { get; set; } = null!;
    private Event<InventoryCheckRequested> InventoryCheckRequested { get; set; } = null!;
    private Event<InventoryCheckCompleted> InventoryCheckCompleted { get; set; } = null!;
}
