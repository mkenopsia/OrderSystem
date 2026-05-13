namespace Shared.Events;

public record OrderCreatedEvent(
    Guid OrderId, 
    string ProductId, 
    int Quantity, 
    Guid UserId,
    string UserEmail,
    DateTime CreatedAtUtc = default);
public record InventoryReservedEvent(
    Guid OrderId, 
    Guid UserId,              // ← Добавили
    string? UserEmail,        // ← Добавили
    DateTime ReservedAtUtc);

public record InventoryFailedEvent(
    Guid OrderId, 
    Guid UserId,              // ← Добавили
    string? UserEmail,        // ← Добавили
    string Reason, 
    DateTime FailedAtUtc);
public record OrderStatusUpdatedEvent(Guid OrderId, string Status, DateTime UpdatedAtUtc);