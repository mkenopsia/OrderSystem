using System.Text.Json;
using Confluent.Kafka;
using InventoryService.Services;
using Shared.Events;

namespace InventoryService.Infrastructure.Kafka;

public class InventoryConsumerWorker(
    IServiceScopeFactory scopeFactory,
    IProducer<string, string> producer,
    IConfiguration config,
    ILogger<InventoryConsumerWorker> logger) : BackgroundService
{
    private const string Topic = "order.events";
    private const string ResponseTopic = "inventory.events";
    private const string GroupId = "inventory-service-group";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            AllowAutoCreateTopics = true
        };

        using var consumer = new ConsumerBuilder<string, string>(consumerConfig)
            .SetErrorHandler((_, e) => logger.LogError("Kafka Consumer Error: {Error}", e.Reason))
            .Build();

        consumer.Subscribe(Topic);
        logger.LogInformation("Subscribed to {Topic} with group {Group}", Topic, GroupId);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(TimeSpan.FromSeconds(2));
                    if (result == null || result.IsPartitionEOF) continue;

                    using var scope = scopeFactory.CreateScope();
                    var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();
                    
                    await ProcessMessageAsync(result.Message, inventoryService, stoppingToken);
                    consumer.Commit(result);
                }
                catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart || 
                                                  ex.Error.Code == ErrorCode.LeaderNotAvailable)
                {
                    logger.LogWarning("Topic {Topic} not ready yet. Waiting...", Topic);
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected consume error. Retrying...");
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
            }
        }
        finally { consumer.Close(); }
    }

    private async Task ProcessMessageAsync(
        Message<string, string> message, 
        IInventoryService inventoryService, 
        CancellationToken ct)
    {
        try
        {
            var evt = JsonSerializer.Deserialize<OrderCreatedEvent>(message.Value);
            if (evt == null) { logger.LogWarning("Failed to deserialize message"); return; }

            var result = await inventoryService.ProcessOrderCreatedAsync(evt, ct);

            if (result == "Reserved")
            {
                var reply = new InventoryReservedEvent(evt.OrderId, evt.UserId, evt.UserEmail, DateTime.UtcNow);
                await PublishAsync(reply, evt.OrderId.ToString(), ct);
            }
            else
            {
                var reply = new InventoryFailedEvent(evt.OrderId, evt.UserId, evt.UserEmail, result, DateTime.UtcNow);
                await PublishAsync(reply, evt.OrderId.ToString(), ct);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing message {Key}", message.Key);
            throw;
        }
    }

    private async Task PublishAsync<T>(T evt, string key, CancellationToken ct)
    {
        var msg = new Message<string, string> { Key = key, Value = JsonSerializer.Serialize(evt) };
        await producer.ProduceAsync(ResponseTopic, msg, ct);
        logger.LogInformation("Published {Event} to {Topic}", typeof(T).Name, ResponseTopic);
    }
}