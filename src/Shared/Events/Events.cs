namespace Shared.Events;

public record OrderCreatedEvent(
    Guid OrderId, 
    string ProductId, 
    int Quantity, 
    Guid UserId,
    string UserEmail,
    DateTime CreatedAtUtc = default);
public record InventoryReservedEvent(Guid OrderId, DateTime ReservedAtUtc);
public record InventoryFailedEvent(Guid OrderId, string Reason, DateTime FailedAtUtc);
public record OrderStatusUpdatedEvent(Guid OrderId, string Status, DateTime UpdatedAtUtc);