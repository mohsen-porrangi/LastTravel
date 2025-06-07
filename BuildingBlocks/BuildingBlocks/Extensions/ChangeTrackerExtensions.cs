using BuildingBlocks.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BuildingBlocks.Extensions
{
    public static class ChangeTrackerExtensions
    {
        public static void SetAuditProperties(this ChangeTracker changeTracker)
        {
            var modifiedEntities = changeTracker.Entries().Where(c => c.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);
            foreach (var entry in modifiedEntities)
            {
                if (entry.State == EntityState.Added)
                {
                    if (entry.Entity.GetType().GetProperty("CreatedAt") != null)
                        entry.Property("CreatedAt").CurrentValue = DateTime.Now;
                }
                if (entry.State == EntityState.Modified)
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTime.Now;
                }
                if (entry.State == EntityState.Deleted && entry.Entity is ISoftDelete)
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTime.Now;

                    entry.Property("IsDeleted").CurrentValue = true;
                    entry.State = EntityState.Modified;
                }
            }
        }
    }
}
