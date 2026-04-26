using NotificationService.Domain;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Repositories;

public class NotificationRepository(NotificationDbContext db) : INotificationRepository
{
    public async Task AddAsync(Notification notification, CancellationToken ct = default) =>
        await db.Notifications.AddAsync(notification, ct);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await db.SaveChangesAsync(ct);
}