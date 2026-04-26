using Microsoft.EntityFrameworkCore;
using InventoryService.Domain;

namespace InventoryService.Infrastructure.Persistence;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.ToTable("inventory_items");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ProductId).IsUnique();
        });

        modelBuilder.Entity<ProcessedEvent>(entity =>
        {
            entity.ToTable("processed_events");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.EventKey).IsUnique();
        });
    }
}