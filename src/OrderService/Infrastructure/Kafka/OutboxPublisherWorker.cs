using Confluent.Kafka;
using OrderService.Repositories;

namespace OrderService.Infrastructure.Kafka;

public class OutboxPublisherWorker(
    IServiceScopeFactory scopeFactory,
    IProducer<string, string> producer,
    ILogger<OutboxPublisherWorker> logger) : BackgroundService
{
    private const string Topic = "order.events";
    private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(3));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _timer.WaitForNextTickAsync(stoppingToken))
        {
            try { await PublishPendingAsync(stoppingToken); }
            catch (Exception ex) { logger.LogError(ex, "Outbox publisher error"); }
        }
    }

    private async Task PublishPendingAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

        var pending = await repo.GetUnpublishedAsync(50, ct);
        if (pending.Count == 0) return;

        var publishedIds = new List<Guid>();

        foreach (var msg in pending)
        {
            try
            {
                await producer.ProduceAsync(Topic, new Message<string, string> { Key = msg.Type, Value = msg.Payload }, ct);
                publishedIds.Add(msg.Id);
            }
            catch (KafkaException ex)
            {
                logger.LogWarning(ex, "Kafka publish failed for {MessageId}", msg.Id);
            }
        }

        if (publishedIds.Count > 0)
        {
            await repo.MarkAsPublishedAsync(publishedIds, ct);
            await repo.SaveChangesAsync(ct);
            logger.LogInformation("Published {Count} outbox messages", publishedIds.Count);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _timer.Dispose();
        await base.StopAsync(cancellationToken);
    }
}