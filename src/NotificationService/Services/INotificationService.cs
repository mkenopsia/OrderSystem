namespace NotificationService.Services;

public interface INotificationService
{
    Task SendAsync(string eventType, string payload, CancellationToken ct = default);
}