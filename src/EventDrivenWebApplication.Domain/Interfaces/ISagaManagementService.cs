using EventDrivenWebApplication.Domain.Entities;

namespace EventDrivenWebApplication.Domain.Interfaces;

public interface ISagaManagementService
{
    Task<OrderProcessState> GetSagaStateByCorrelationIdAsync(Guid correlationId);
}