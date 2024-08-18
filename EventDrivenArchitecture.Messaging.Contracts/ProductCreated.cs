namespace EventDrivenArchitecture.Messaging.Contracts;

public record ProductCreated
{
    public int Id { get; init; }
    public Guid ProductId { get; init; }
    public required string Name { get; init; }
    public int Quantity { get; init; }
}