namespace UserAccessManagement.Domain.Users.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task AddAsync(User user);
        Task<IEnumerable<User>> GetAllAsync(int pageNumber, int pageSize);
        Task<int> CountAsync();
        Task<User?> GetByIdAsync(Guid id);
    }
}
