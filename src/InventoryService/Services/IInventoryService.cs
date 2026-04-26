namespace InventoryService.Services;

public interface IInventoryService
{
    Task<string> ProcessOrderCreatedAsync(Shared.Events.OrderCreatedEvent evt, CancellationToken ct);
}