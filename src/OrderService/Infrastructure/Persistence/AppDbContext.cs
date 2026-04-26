using Microsoft.EntityFrameworkCore;
using OrderService.Domain;

namespace OrderService.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(e =>
        {
            e.ToTable("orders");
            e.HasKey(o => o.Id);
            e.HasIndex(o => o.ProductId);
        });

        modelBuilder.Entity<OutboxMessage>(e =>
        {
            e.ToTable("outbox_messages");
            e.HasKey(o => o.Id);
            e.HasIndex(o => new { o.IsPublished, o.OccurredOnUtc });
        });
    }
}