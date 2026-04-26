using NotificationService.Domain;
using NotificationService.Repositories;

namespace NotificationService.Services;

public class NotificationService(INotificationRepository repo, ILogger<NotificationService> logger) : INotificationService
{
    public async Task SendAsync(string eventType, string payload, CancellationToken ct = default)
    {
        var notification = new Notification { EventType = eventType, Message = payload };
        await repo.AddAsync(notification, ct);
        await repo.SaveChangesAsync(ct);
        
        logger.LogInformation("📩 Notification saved: {EventType} | Payload: {Payload}", eventType, payload);
    }
}