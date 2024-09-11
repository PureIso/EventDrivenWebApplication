namespace EventDrivenWebApplication.Domain.Entities;

public class InventoryCheckLog
{
    public int Id { get; set; }
    public Guid ProductId { get; set; }
    public int TotalUniqueItems { get; set; }
    public int TotalQuantity { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}