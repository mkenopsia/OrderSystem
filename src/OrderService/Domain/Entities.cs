namespace OrderService.Domain;

public class Order
{
    public Guid Id { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime OccurredOnUtc { get; set; } = DateTime.UtcNow;
    public bool IsPublished { get; set; }
}