using Microsoft.EntityFrameworkCore;
using OrderService.Domain;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Repositories;

public class OrderRepository(AppDbContext db) : IOrderRepository
{
    public async Task AddAsync(Order order, CancellationToken ct = default) =>
        await db.Orders.AddAsync(order, ct);

    public async Task AddOutboxMessageAsync(OutboxMessage message, CancellationToken ct = default) =>
        await db.OutboxMessages.AddAsync(message, ct);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await db.SaveChangesAsync(ct);

    public async Task<List<OutboxMessage>> GetUnpublishedAsync(int limit, CancellationToken ct = default) =>
        await db.OutboxMessages
            .Where(m => !m.IsPublished)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(limit)
            .ToListAsync(ct);

    public async Task MarkAsPublishedAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var messages = await db.OutboxMessages.Where(m => ids.Contains(m.Id)).ToListAsync(ct);
        foreach (var m in messages) m.IsPublished = true;
    }
}