namespace InventoryService.Domain;

public class InventoryItem
{
    public Guid Id { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
}

public class ProcessedEvent
{
    public Guid Id { get; set; }
    public string EventKey { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime ProcessedAtUtc { get; set; } = DateTime.UtcNow;
}