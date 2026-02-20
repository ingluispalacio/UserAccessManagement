

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;


namespace UserAccessManagement.Infrastructure.Persistence.Context
{
    public class UserAccessDbContextFactory
    : IDesignTimeDbContextFactory<UserAccessDbContext>
    {
        public UserAccessDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<UserAccessDbContext>();

            optionsBuilder.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"));

            return new UserAccessDbContext(optionsBuilder.Options);
        }
    }

}
