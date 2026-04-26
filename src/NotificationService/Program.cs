using Microsoft.EntityFrameworkCore;
using NotificationService.Infrastructure.Kafka;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Repositories;
using NotificationService.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<NotificationDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService.Services.NotificationService>();
builder.Services.AddHostedService<NotificationConsumerWorker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    await db.Database.EnsureDeletedAsync();
    await db.Database.EnsureCreatedAsync();
}

host.Run();