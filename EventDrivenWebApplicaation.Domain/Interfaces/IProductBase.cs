namespace EventDrivenWebApplication.Domain.Interfaces;

public interface IProductBase
{
    int Id { get; }
    Guid ProductId { get; }
    string Name { get; }
    int Quantity { get; }
    decimal Price { get; }
}