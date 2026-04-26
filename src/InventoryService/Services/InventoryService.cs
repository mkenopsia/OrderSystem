using InventoryService.Domain;
using InventoryService.Repositories;
using Microsoft.Extensions.Logging;

namespace InventoryService.Services;

public class InventoryService(IInventoryRepository repo, ILogger<InventoryService> logger) : IInventoryService
{
    public async Task<string> ProcessOrderCreatedAsync(Shared.Events.OrderCreatedEvent evt, CancellationToken ct)
    {
        if (await repo.IsEventProcessedAsync(evt.OrderId.ToString(), ct))
        {
            logger.LogInformation("Event {OrderId} already processed, skipping.", evt.OrderId);
            return "AlreadyProcessed";
        }

        var item = await repo.GetByProductIdAsync(evt.ProductId, ct);
        if (item == null)
        {
            await MarkProcessedAsync(evt.OrderId.ToString(), ct);
            return "Failed";
        }

        if (item.Quantity >= evt.Quantity)
        {
            item.Quantity -= evt.Quantity;
            item.LastUpdatedUtc = DateTime.UtcNow;
            await MarkProcessedAsync(evt.OrderId.ToString(), ct);
            await repo.SaveChangesAsync(ct);
            logger.LogInformation("Reserved {Qty} of {ProductId}. Remaining: {Remaining}", evt.Quantity, evt.ProductId, item.Quantity);
            return "Reserved";
        }

        await MarkProcessedAsync(evt.OrderId.ToString(), ct);
        logger.LogWarning("Insufficient inventory for {ProductId}. Requested: {Req}, Available: {Avail}", evt.ProductId, evt.Quantity, item.Quantity);
        return "Failed";
    }

    private async Task MarkProcessedAsync(string eventKey, CancellationToken ct)
    {
        await repo.AddProcessedEventAsync(new ProcessedEvent
        {
            Id = Guid.NewGuid(),
            EventKey = eventKey,
            EventType = nameof(Shared.Events.OrderCreatedEvent)
        }, ct);
    }
}