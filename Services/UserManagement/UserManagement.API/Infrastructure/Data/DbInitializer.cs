namespace UserManagement.API.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static void Seed(UserDbContext db)
        {
            if (!db.Permissions.Any())
            {
                var permissions = Seeders.PermissionSeeder.Seed();
                db.Permissions.AddRange(permissions);
            }

            // سایر seeding‌ها در آینده می‌تونن اینجا اضافه بشن

            db.SaveChanges();
        }
    }
}
