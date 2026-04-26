using Microsoft.EntityFrameworkCore;
using NotificationService.Domain;

namespace NotificationService.Infrastructure.Persistence;

public class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(e =>
        {
            e.ToTable("notifications");
            e.HasKey(n => n.Id);
        });
    }
}