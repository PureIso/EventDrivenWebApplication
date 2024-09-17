using EventDrivenWebApplication.Domain.Entities;

namespace EventDrivenWebApplication.Domain.Interfaces;

/// <summary>
/// Interface for managing order process saga states.
/// </summary>
public interface IOrderProcessStateService
{
    /// <summary>
    /// Gets the current state of the order process by correlation ID.
    /// </summary>
    /// <param name="correlationId">The correlation ID of the saga.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The current order process state, or null if not found.</returns>
    Task<OrderProcessState?> GetOrderProcessStateAsync(Guid correlationId, CancellationToken cancellationToken);
}