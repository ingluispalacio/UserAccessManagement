

namespace UserAccessManagement.Application.Auth.DTOs
{
    public record RegisterResponse(
       Guid Id,
       string Name,
       string Lastname,
       string Email,
       string Address,
       bool IsActive
   );
}
