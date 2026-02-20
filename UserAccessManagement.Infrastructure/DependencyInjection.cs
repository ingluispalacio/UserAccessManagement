using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserAccessManagement.Application.Interfaces;
using UserAccessManagement.Domain.Users.Interfaces;
using UserAccessManagement.Infrastructure.Persistence;
using UserAccessManagement.Infrastructure.Persistence.Context;
using UserAccessManagement.Infrastructure.Persistence.Repositories;
using UserAccessManagement.Infrastructure.Security;

namespace UserAccessManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<UserAccessDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));
            // 🔹 Repositories
            services.AddScoped<IUserRepository, UserRepository>();

            // 🔹 Unit Of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // 🔹 Password Hasher
            services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();

            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            return services;
        }
    }
}
