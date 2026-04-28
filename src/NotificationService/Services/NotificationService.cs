using System.Text.Json;
using NotificationService.Domain;
using NotificationService.Repositories;

namespace NotificationService.Services;

public class NotificationService(
    INotificationRepository repo, 
    IEmailSender emailSender, 
    ILogger<NotificationService> logger) : INotificationService
{
    public async Task SendAsync(string eventType, string payload, CancellationToken ct = default)
    {
        var notification = new Notification { EventType = eventType, Message = payload };
        await repo.AddAsync(notification, ct);
        await repo.SaveChangesAsync(ct);
        
        try
        {
            var email = ExtractEmailFromPayload(payload);
            if (!string.IsNullOrEmpty(email))
            {
                var subject = GetSubjectForEvent(eventType);
                var body = GetBodyForEvent(eventType, payload);
                await emailSender.SendAsync(email, subject, body, ct);
            }
            else
            {
                logger.LogWarning("Could not extract email from payload for {EventType}", eventType);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email notification for {EventType}", eventType);
        }

        logger.LogInformation("📩 Notification processed: {EventType}", eventType);
    }

    private string? ExtractEmailFromPayload(string payload)
    {
        try
        {
            using var doc = JsonDocument.Parse(payload);
            if (doc.RootElement.TryGetProperty("UserEmail", out var prop))
                return prop.GetString();
            
            if (doc.RootElement.TryGetProperty("UserId", out _) && 
                doc.RootElement.TryGetProperty("UserEmail", out prop))
                return prop.GetString();
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Invalid JSON payload during email extraction");
        }
        return null;
    }

    private string GetSubjectForEvent(string eventType) => eventType switch
    {
        "InventoryReservedEvent" => "✅ Ваш заказ подтверждён!",
        "InventoryFailedEvent"   => "⚠️ Проблема с заказом",
        _                        => "📦 Уведомление о заказе"
    };

    private string GetBodyForEvent(string eventType, string payload) =>
        $"Событие: {eventType}\nДетали: {payload}\n\nСпасибо, что вы с нами!";
}