using Microsoft.EntityFrameworkCore;
using OrderStateMachineOutboxDemo.Models;

namespace OrderStateMachineOutboxDemo.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CustomerId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ProductSku).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<OutboxEvent>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("text").IsRequired();
        });
    }
}
