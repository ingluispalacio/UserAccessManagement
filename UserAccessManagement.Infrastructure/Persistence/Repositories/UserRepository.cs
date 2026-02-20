using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using UserAccessManagement.Domain.Users;
using UserAccessManagement.Domain.Users.Interfaces;
using UserAccessManagement.Infrastructure.Persistence.Context;

namespace UserAccessManagement.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserAccessDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(
           UserAccessDbContext context,
           ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User?> GetByEmailAsync(string email)
            => await _context.Users.FirstOrDefaultAsync(x => x.Email.Value == email);

        public async Task AddAsync(User user)
            => await _context.Users.AddAsync(user);

        public async Task<IEnumerable<User>> GetAllAsync(int pageNumber, int pageSize)
        => await _context.Users
                .AsNoTracking()
                .OrderBy(u => u.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


        public async Task<int> CountAsync()
        => await _context.Users.CountAsync();


        public async Task<User?> GetByIdAsync(Guid id)
        => await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);


    }
}
