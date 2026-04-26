using Confluent.Kafka;
using NotificationService.Services;
using Shared.Events;

namespace NotificationService.Infrastructure.Kafka;

public class NotificationConsumerWorker(
    IServiceScopeFactory scopeFactory,
    IConfiguration config,
    ILogger<NotificationConsumerWorker> logger) : BackgroundService
{
    private const string Topic = "inventory.events";
    private const string GroupId = "notification-service-group";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnablePartitionEof = true
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig)
            .SetErrorHandler((_, e) => logger.LogError("Kafka Error: {Reason}", e.Reason))
            .Build();

        consumer.Subscribe(Topic);
        logger.LogInformation("Subscribed to topic {Topic}", Topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(TimeSpan.FromSeconds(1));

                    if (result == null) continue;
                    if (result.IsPartitionEOF) continue;
                    
                    logger.LogInformation("Received message: {Value}", result.Message.Value); //todo
                    
                }
                catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
                {
                    logger.LogWarning("Topic {Topic} not found yet. Waiting for creation...", Topic);
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error during consumption");
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
            }
        }
        finally
        {
            consumer.Close();
            logger.LogInformation("Consumer closed.");
        }
    }

    private async Task ProcessMessageAsync(
        Message<string, string> message, 
        INotificationService service, 
        CancellationToken ct)
    {
        var eventType = message.Value.Contains("ReservedAtUtc") 
            ? nameof(InventoryReservedEvent) 
            : nameof(InventoryFailedEvent);

        await service.SendAsync(eventType, message.Value, ct);
    }
}