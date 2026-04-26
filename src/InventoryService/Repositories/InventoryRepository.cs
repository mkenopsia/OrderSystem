using Microsoft.EntityFrameworkCore;
using InventoryService.Domain;
using InventoryService.Infrastructure.Persistence;

namespace InventoryService.Repositories;

public class InventoryRepository(InventoryDbContext db) : IInventoryRepository
{
    public async Task<InventoryItem?> GetByProductIdAsync(string productId, CancellationToken ct) =>
        await db.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == productId, ct);

    public async Task AddProcessedEventAsync(ProcessedEvent evt, CancellationToken ct) =>
        await db.ProcessedEvents.AddAsync(evt, ct);

    public async Task<bool> IsEventProcessedAsync(string eventKey, CancellationToken ct) =>
        await db.ProcessedEvents.AnyAsync(e => e.EventKey == eventKey, ct);

    public async Task<int> SaveChangesAsync(CancellationToken ct) =>
        await db.SaveChangesAsync(ct);
}