using Microsoft.EntityFrameworkCore;
using UserAccessManagement.Domain.Users;

namespace UserAccessManagement.Infrastructure.Persistence.Context
{
    public class UserAccessDbContext : DbContext
    {
        public UserAccessDbContext(DbContextOptions<UserAccessDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserAccessDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
