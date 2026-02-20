using MediatR;
using UserAccessManagement.Application.Common;

namespace UserAccessManagement.Application.Users.Commands.UpdateUser
{
    public record UpdateUserCommand
     (
        Guid Id,
        string Name,
        string Lastname,
        string Email,
        string? Address
    ) : IRequest<ApiResponse<Unit>>;
}
