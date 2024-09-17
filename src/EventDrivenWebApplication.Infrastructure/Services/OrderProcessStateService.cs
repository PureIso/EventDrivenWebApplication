using EventDrivenWebApplication.Domain.Entities;
using EventDrivenWebApplication.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventDrivenWebApplication.Infrastructure.Services;

/// <summary>
/// Service to manage the order process saga states.
/// </summary>
public class OrderProcessStateService : IOrderProcessStateService
{
    private readonly OrderSagaDbContext _dbContext;
    private readonly ILogger<OrderProcessStateService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderProcessStateService"/> class.
    /// </summary>
    /// <param name="dbContext">The database context to interact with saga states.</param>
    /// <param name="logger">The logger to log actions in the service.</param>
    public OrderProcessStateService(OrderSagaDbContext dbContext, ILogger<OrderProcessStateService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current state of the order process by correlation ID.
    /// </summary>
    /// <param name="correlationId">The correlation ID of the saga.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The current order process state, or null if not found.</returns>
    public async Task<OrderProcessState?> GetOrderProcessStateAsync(Guid correlationId, CancellationToken cancellationToken)
    {
        return await _dbContext.OrderProcessStates
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.CorrelationId == correlationId, cancellationToken);
    }
}
