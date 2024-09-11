using EventDrivenWebApplication.Domain.Entities;
using EventDrivenWebApplication.Domain.Interfaces;
using EventDrivenWebApplication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventDrivenWebApplication.Infrastructure.Services;

public class SagaManagementService : ISagaManagementService
{
    private readonly OrderSagaDbContext _context;

    public SagaManagementService(OrderSagaDbContext context)
    {
        _context = context;
    }

    public async Task<OrderProcessState> GetSagaStateByCorrelationIdAsync(Guid correlationId)
    {
        return await _context.OrderProcessStates
            .SingleOrDefaultAsync(s => s.CorrelationId == correlationId);
    }
}