namespace Shared.Events;

public record OrderCreatedEvent(Guid OrderId, string ProductId, int Quantity, DateTime CreatedAtUtc);
public record InventoryReservedEvent(Guid OrderId, DateTime ReservedAtUtc);
public record InventoryFailedEvent(Guid OrderId, string Reason, DateTime FailedAtUtc);
public record OrderStatusUpdatedEvent(Guid OrderId, string Status, DateTime UpdatedAtUtc);