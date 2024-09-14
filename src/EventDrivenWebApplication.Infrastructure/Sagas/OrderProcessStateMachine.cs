using EventDrivenWebApplication.Domain.Entities;
using EventDrivenWebApplication.Infrastructure.Messaging.Contracts;
using MassTransit;
using Serilog;

/// <summary>
/// State machine for processing orders in an event-driven system.
/// Manages the order process through states including creation, inventory check request, and completion.
/// </summary>
public class OrderProcessStateMachine : MassTransitStateMachine<OrderProcessState>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrderProcessStateMachine"/> class.
    /// Configures the state machine to handle events and transitions between states.
    /// </summary>
    public OrderProcessStateMachine()
    {
        // Define the current state of the saga instance
        InstanceState(x => x.CurrentState);

        // Define the events and their correlation
        Event(() => ProductCreated, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => InventoryCheckRequested, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => InventoryCheckCompleted, x => x.CorrelateById(context => context.Message.CorrelationId));

        // Initial state: handle ProductCreated event
        Initially(
            When(ProductCreated)
                .Then(context =>
                {
                    context.Saga.CorrelationId = context.Message.CorrelationId;
                    context.Saga.ProductId = context.Message.ProductId;
                    context.Saga.ProductName = context.Message.Name;
                    context.Saga.ProductQuantity = context.Message.Quantity;
                    context.Saga.Price = context.Message.Price;
                    context.Saga.DateTimeProductCreated = context.Message.DateTimeCreated;
                    Log.Information($"Transitioning to WaitingForInventoryCheckRequest ProductId: {context.Message.ProductId}");
                })
                .TransitionTo(WaitingForInventoryCheckRequest),
        When(InventoryCheckRequested)
            .Then(context =>
            {
                // Handle logic for when an inventory check is requested
                context.Saga.CorrelationId = context.Message.CorrelationId;
                context.Saga.DateTimeInventoryCheckRequested = context.Message.DateTimeCreated;
                Log.Information($"Received InventoryCheckRequested for ProductID: {context.Message.ProductId}. Transitioning to InventoryCheckRequestedState.");
            })
            .TransitionTo(InventoryCheckRequestedState),
        When(InventoryCheckCompleted)
            .Then(context =>
            {
                // Update the saga state with inventory availability and check details
                context.Saga.CorrelationId = context.Message.CorrelationId;
                context.Saga.IsQualityGood = context.Message.IsQualityGood;
                context.Saga.DateTimeInventoryCheckCompleted = context.Message.DateTimeInventoryCompleted;
                Log.Information($"InventoryCheckCompleted received for ProductID: {context.Message.ProductId}, IsQualityGood: {context.Message.IsQualityGood}. Transitioning to Completed state.");
            })
            // Transition to the completed state when inventory check is done
            .TransitionTo(Completed)
        );

        // Handle ProductCreated if received again (out of order or duplicate)
        During(WaitingForInventoryCheckRequest,
            When(ProductCreated)
                .Then(context =>
                {
                    Log.Warning($"Duplicate or out-of-order ProductCreated event ignored. ProductID: {context.Message.ProductId}, CurrentState: {context.Saga.CurrentState}");
                })
        );

        // InventoryCheckRequested can only be handled when in the WaitingForInventoryCheckRequest state
        During(WaitingForInventoryCheckRequest,
            When(InventoryCheckRequested)
                .Then(context =>
                {
                    // Handle logic for when an inventory check is requested
                    context.Saga.CorrelationId = context.Message.CorrelationId;
                    context.Saga.DateTimeInventoryCheckRequested = context.Message.DateTimeCreated;
                    Log.Information($"Received InventoryCheckRequested for ProductID: {context.Message.ProductId}. Transitioning to InventoryCheckRequestedState.");
                })
                .TransitionTo(InventoryCheckRequestedState) // Transition to the state where the inventory check is happening
        );

        // Handle duplicate or out-of-order InventoryCheckRequested in the wrong state
        During(InventoryCheckRequestedState,
            When(InventoryCheckRequested)
                .Then(context =>
                {
                    Log.Warning($"Duplicate or out-of-order InventoryCheckRequested event ignored. ProductID: {context.Message.ProductId}, CurrentState: {context.Saga.CurrentState}");
                })
        );

        // Handle the InventoryCheckCompleted event in the correct state
        During(InventoryCheckRequestedState,
            When(InventoryCheckCompleted)
                .Then(context =>
                {
                    // Update the saga state with inventory availability and check details
                    context.Saga.CorrelationId = context.Message.CorrelationId;
                    context.Saga.IsQualityGood = context.Message.IsQualityGood;
                    context.Saga.DateTimeInventoryCheckCompleted = context.Message.DateTimeInventoryCompleted;
                    Log.Information($"InventoryCheckCompleted received for ProductID: {context.Message.ProductId}, IsQualityGood: {context.Message.IsQualityGood}. Transitioning to Completed state.");
                })
                // Transition to the completed state when inventory check is done
                .TransitionTo(Completed)
        );

        // If InventoryCheckCompleted is received out of order, log a warning
        During(WaitingForInventoryCheckRequest,
            When(InventoryCheckCompleted)
                .Then(context =>
                {
                    Log.Warning($"Out-of-order InventoryCheckCompleted event ignored. ProductID: {context.Message.ProductId}, CurrentState: {context.Saga.CurrentState}");
                })
        );

        // Mark the saga as completed when in the final state
        SetCompletedWhenFinalized();
    }

    /// <summary>
    /// Gets the state representing the creation of the order.
    /// </summary>
    public State CreatedState { get; private set; }

    /// <summary>
    /// Gets the state representing waiting for an inventory check request.
    /// </summary>
    public State WaitingForInventoryCheckRequest { get; private set; }

    /// <summary>
    /// Gets the state representing that an inventory check has been requested.
    /// </summary>
    public State InventoryCheckRequestedState { get; private set; }

    /// <summary>
    /// Gets the state representing the completion of the order process.
    /// </summary>
    public State Completed { get; private set; }

    /// <summary>
    /// Gets the event for when a product is created.
    /// </summary>
    public Event<ProductCreatedMessage> ProductCreated { get; private set; }

    /// <summary>
    /// Gets the event for when an inventory check is requested.
    /// </summary>
    public Event<InventoryCheckRequested> InventoryCheckRequested { get; private set; }

    /// <summary>
    /// Gets the event for when an inventory check is completed.
    /// </summary>
    public Event<InventoryCheckCompleted> InventoryCheckCompleted { get; private set; }
}
