
//using EventDrivenWebApplication.Domain.Entities;
//using MassTransit;

//public class SagaRepository : ISagaRepository<OrderProcessState>
//{
//    private readonly InMemorySagaRepository<OrderProcessState> _repository;

//    public SagaRepository(InMemorySagaRepository<OrderProcessState> repository)
//    {
//        _repository = repository;
//    }

//    public async Task<OrderProcessState?> GetSagaByCustomerIdAsync(Guid customerId)
//    {
//        // Implement logic to retrieve saga state by CustomerId
//        return await _repository.GetSagaByIdAsync(customerId.ToString()); // Adjust to match your implementation
//    }

//    public async Task UpdateSagaAsync(OrderProcessState saga)
//    {
//        await _repository.UpdateAsync(saga); // Adjust to match your implementation
//    }
//}