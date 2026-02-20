using UserAccessManagement.Domain.Users;

namespace UserAccessManagement.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
    }

}
