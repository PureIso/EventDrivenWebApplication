using MassTransit;
using EventDrivenWebApplication.Domain.Entities;
using EventDrivenWebApplication.Infrastructure.Messaging.Contracts;

namespace EventDrivenWebApplication.Infrastructure.Sagas;

public class OrderProcessStateMachine : MassTransitStateMachine<OrderProcessState>
{
    public OrderProcessStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => ProductCreated, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => InventoryCheckRequested, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => InventoryCheckCompleted, x => x.CorrelateById(context => context.Message.OrderId));

        Initially(
            When(ProductCreated)
                .Then(context =>
                {
                    context.Saga.ProductId = context.Message.ProductId;
                    context.Saga.OrderId = context.Message.OrderId;
                })
                .TransitionTo(InventoryCheckRequestedState)
                .Publish(context => new InventoryCheckRequested
                {
                    OrderId = context.Saga.OrderId,
                    ProductId = context.Saga.ProductId,
                    Quantity = context.Message.Quantity
                })
        );

        During(InventoryCheckRequestedState,
            When(InventoryCheckCompleted)
                .Then(context =>
                {
                    context.Saga.IsInventoryAvailable = context.Message.IsAvailable;
                    context.Saga.IsInventoryChecked = true;
                    context.Saga.InventoryCheckedAt = context.Message.CheckedAt;
                })
                .TransitionTo(Completed)
        );

        SetCompletedWhenFinalized();
    }

    public State InventoryCheckRequestedState { get; private set; }
    public State Completed { get; private set; }

    public Event<ProductCreatedMessage> ProductCreated { get; private set; }
    public Event<InventoryCheckRequested> InventoryCheckRequested { get; private set; }
    public Event<InventoryCheckCompleted> InventoryCheckCompleted { get; private set; }
}