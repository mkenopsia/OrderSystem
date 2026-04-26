using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using InventoryService.Domain;
using InventoryService.Infrastructure.Kafka;
using InventoryService.Infrastructure.Persistence;
using InventoryService.Repositories;
using InventoryService.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<InventoryDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IInventoryService, InventoryService.Services.InventoryService>();

var kafkaProdCfg = new ProducerConfig
{
    BootstrapServers = builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
    Acks = Acks.All,
    EnableIdempotence = true,
    MessageSendMaxRetries = 3,
    RetryBackoffMs = 100
};
builder.Services.AddSingleton<IProducer<string, string>>(new ProducerBuilder<string, string>(kafkaProdCfg).Build());
builder.Services.AddHostedService<InventoryConsumerWorker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    await db.Database.EnsureDeletedAsync();
    await db.Database.EnsureCreatedAsync();
    
    if (!await db.InventoryItems.AnyAsync())
    {
        db.InventoryItems.AddRange(
            new InventoryItem { Id = Guid.NewGuid(), ProductId = "LAPTOP-001", Quantity = 10 },
            new InventoryItem { Id = Guid.NewGuid(), ProductId = "PHONE-002", Quantity = 3 }
        );
        await db.SaveChangesAsync();
    }
}

host.Run();