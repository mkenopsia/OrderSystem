using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Kafka;
using OrderService.Infrastructure.Persistence;
using OrderService.Repositories;
using OrderService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService.Services.OrderService>();

var kafkaConfig = new ProducerConfig
{
    BootstrapServers = builder.Configuration.GetValue("Kafka:BootstrapServers", "kafka:9092"),
    Acks = Acks.All,
    EnableIdempotence = true,
    MessageSendMaxRetries = 3,
    RetryBackoffMs = 100
};
builder.Services.AddSingleton<IProducer<string, string>>(new ProducerBuilder<string, string>(kafkaConfig).Build());
builder.Services.AddHostedService<OutboxPublisherWorker>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    await db.Database.EnsureDeletedAsync();
    await db.Database.EnsureCreatedAsync();
}

app.Run();