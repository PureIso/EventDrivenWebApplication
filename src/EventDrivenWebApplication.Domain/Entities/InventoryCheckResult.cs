namespace EventDrivenWebApplication.Domain.Entities;

public class InventoryCheckResult
{
    public bool IsAvailable { get; set; }
    public int Quantity { get; set; }
}