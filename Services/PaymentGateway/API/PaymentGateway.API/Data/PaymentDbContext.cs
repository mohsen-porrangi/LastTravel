using Microsoft.EntityFrameworkCore;

namespace PaymentGateway.API.Data;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(p => p.Description)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(p => p.Authority)
                .HasMaxLength(100);

            entity.Property(p => p.ReferenceId)
                .HasMaxLength(100);

            entity.Property(p => p.CallbackUrl)
                .HasMaxLength(500);

            entity.Property(p => p.OrderId)
                .HasMaxLength(100);

            entity.Property(p => p.ErrorMessage)
                .HasMaxLength(500);

            entity.Property(p => p.RefundTrackingId)
                .HasMaxLength(100);

            entity.Property(p => p.AdditionalData)
                .HasMaxLength(4000);

            entity.HasQueryFilter(p => !p.IsDeleted);
        });
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<Payment>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
    }
}