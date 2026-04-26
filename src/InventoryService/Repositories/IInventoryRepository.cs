using InventoryService.Domain;

namespace InventoryService.Repositories;

public interface IInventoryRepository
{
    Task<InventoryItem?> GetByProductIdAsync(string productId, CancellationToken ct);
    Task AddProcessedEventAsync(ProcessedEvent evt, CancellationToken ct);
    Task<bool> IsEventProcessedAsync(string eventKey, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
}