using MassTransit;

namespace EventDrivenWebApplication.Domain.Entities;

/// <summary>
/// Represents the history of state transitions for an order process in the saga.
/// </summary>
public class OrderProcessStateHistory : SagaStateMachineInstance
{
    /// <summary>
    /// Gets or sets the primary key of the history entry.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the correlation ID that uniquely identifies the order process.
    /// </summary>
    public Guid CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the previous state before the transition.
    /// </summary>
    public string PreviousState { get; set; } = "Unknown";

    /// <summary>
    /// Gets or sets the current state after the transition.
    /// </summary>
    public string CurrentState { get; set; } = "Unknown";

    /// <summary>
    /// Gets or sets the timestamp of when the state transition occurred.
    /// </summary>
    public DateTime TransitionedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the description or details of the event that triggered the transition.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}