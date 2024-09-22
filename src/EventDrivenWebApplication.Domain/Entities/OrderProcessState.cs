using MassTransit;

namespace EventDrivenWebApplication.Domain.Entities;

/// <summary>
/// Represents the state of an order process within the saga, implementing the SagaStateMachineInstance interface.
/// </summary>
public class OrderProcessState : SagaStateMachineInstance
{
    /// <summary>
    /// Gets or sets the unique identifier for the correlation of the saga instance.
    /// </summary>
    public Guid CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the current state of the order process.
    /// </summary>
    public string CurrentState { get; set; } = "Initial";

    /// <summary>
    /// Gets or sets the previous state of the order process.
    /// </summary>
    public string PreviousState { get; set; } = "Initial";

    /// <summary>
    /// Gets or sets the identifier of the product associated with the order.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the name of the product associated with the order.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity of the product in the order.
    /// </summary>
    public int ProductQuantity { get; set; }

    /// <summary>
    /// Gets or sets the price of the product in the order.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the quality of the product is good.
    /// </summary>
    public bool IsQualityGood { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the product was created.
    /// </summary>
    public DateTime DateTimeProductCreated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the inventory check was requested.
    /// </summary>
    public DateTime DateTimeInventoryCheckRequested { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the inventory check was completed.
    /// </summary>
    public DateTime DateTimeInventoryCheckCompleted { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the row version for optimistic concurrency control.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
