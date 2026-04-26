using OrderService.Domain;

namespace OrderService.Repositories;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken ct = default);
    Task AddOutboxMessageAsync(OutboxMessage message, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<List<OutboxMessage>> GetUnpublishedAsync(int limit, CancellationToken ct = default);
    Task MarkAsPublishedAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
}