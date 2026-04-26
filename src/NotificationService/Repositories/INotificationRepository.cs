using NotificationService.Domain;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Repositories;

public interface INotificationRepository
{
    Task AddAsync(Notification notification, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}