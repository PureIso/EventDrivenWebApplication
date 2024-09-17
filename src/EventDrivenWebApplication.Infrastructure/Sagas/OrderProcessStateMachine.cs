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

        // Define initial state transition
        Initially(
            When(ProductCreated)
                .Then(context => context.Saga.PreviousState = context.Saga.CurrentState) // Capture PreviousState
                .Then(ProcessProductCreated)
                .ThenAsync(context => LogStateTransitionAsync(context, "ProductCreated event processed", "Exit"))
                .TransitionTo(WaitingForInventoryCheckRequest),

            When(InventoryCheckRequested)
                .Then(context => LogWarning(context, "InventoryCheckRequested received but no ProductCreated event has been processed."))
        );

        // Define state transitions during WaitingForInventoryCheckRequest state
        During(WaitingForInventoryCheckRequest,
            When(ProductCreated)
                .Then(context => LogWarning(context, "Duplicate or out-of-order ProductCreated event ignored.")),

            When(InventoryCheckRequested)
                .Then(context => context.Saga.PreviousState = context.Saga.CurrentState) // Capture PreviousState
                .Then(ProcessInventoryCheckRequested)
                .ThenAsync(context => LogStateTransitionAsync(context, "InventoryCheckRequested event processed", "Exit"))
                .TransitionTo(InventoryCheckRequestedState),

            When(InventoryCheckCompleted)
                .Then(context => LogWarning(context, "Out-of-order InventoryCheckCompleted event ignored."))
        );

        // Define state transitions during InventoryCheckRequestedState state
        During(InventoryCheckRequestedState,
            When(ProductCreated)
                .Then(context => LogWarning(context, "Duplicate or out-of-order ProductCreated event ignored.")),

            When(InventoryCheckRequested)
                .Then(context => LogWarning(context, "Duplicate or out-of-order InventoryCheckRequested event ignored.")),

            When(InventoryCheckCompleted)
                .Then(context => context.Saga.PreviousState = context.Saga.CurrentState) // Capture PreviousState
                .Then(ProcessInventoryCheckCompleted)
                .ThenAsync(context => LogStateTransitionAsync(context, "InventoryCheckCompleted event processed", "Exit"))
                .TransitionTo(Completed)
        );

        // Handle out-of-order InventoryCheckCompleted during WaitingForInventoryCheckRequest
        During(WaitingForInventoryCheckRequest,
            When(InventoryCheckCompleted)
                .Then(context => LogWarning(context, "Out-of-order InventoryCheckCompleted event ignored."))
        );

        // Mark the saga as completed when it reaches the final state
        SetCompletedWhenFinalized();
    }

    // Method to process ProductCreated event
    private async void ProcessProductCreated(BehaviorContext<OrderProcessState, ProductCreatedMessage> context)
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

    // Method to process InventoryCheckRequested event
    private async void ProcessInventoryCheckRequested(BehaviorContext<OrderProcessState, InventoryCheckRequested> context)
    {
        await LogStateTransitionAsync(context, "InventoryCheckRequested event processed", "Entry");

        context.Saga.CorrelationId = context.Message.CorrelationId;
        context.Saga.DateTimeInventoryCheckRequested = context.Message.DateTimeCreated;

        Log.Information($"InventoryCheckRequested event processed for ProductID: {context.Message.ProductId}. Transitioning to InventoryCheckRequestedState.");
    }

    // Method to process InventoryCheckCompleted event
    private async void ProcessInventoryCheckCompleted(BehaviorContext<OrderProcessState, InventoryCheckCompleted> context)
    {
        await LogStateTransitionAsync(context, "InventoryCheckCompleted event processed", "Entry");

        context.Saga.CorrelationId = context.Message.CorrelationId;
        context.Saga.IsQualityGood = context.Message.IsQualityGood;
        context.Saga.DateTimeInventoryCheckCompleted = context.Message.DateTimeInventoryCompleted;

        Log.Information($"InventoryCheckCompleted event processed for ProductID: {context.Message.ProductId}. Transitioning to Completed.");
    }

    // Method to log state transition
    private async Task LogStateTransitionAsync<TEvent>(BehaviorContext<OrderProcessState, TEvent> context, string description, string entryOrExit)
        where TEvent : class
    {
        using IServiceScope scope = context.GetPayload<IServiceScopeFactory>().CreateScope();
        OrderSagaDbContext dbContext = scope.ServiceProvider.GetRequiredService<OrderSagaDbContext>();

        OrderProcessStateHistory historyEntry = new OrderProcessStateHistory
        {
            CorrelationId = context.Saga.CorrelationId,
            PreviousState = context.Saga.PreviousState,  // Set previous state
            CurrentState = context.Saga.CurrentState,    // Set current state
            TransitionedAt = DateTime.UtcNow,
            Description = $"{description} - {entryOrExit}"
        };

        dbContext.OrderProcessStateHistories.Add(historyEntry);
        await dbContext.SaveChangesAsync();
    }

    // Method to log warnings
    private void LogWarning<TEvent>(BehaviorContext<OrderProcessState, TEvent> context, string message)
        where TEvent : class
    {
        string productId = context.Saga.ProductId != 0 ? context.Saga.ProductId.ToString() : "Unknown";
        Log.Warning($"{message} ProductID: {productId}, CurrentState: {context.Saga.CurrentState}");
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
