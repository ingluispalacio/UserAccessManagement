using MediatR;
using UserAccessManagement.Application.Common;
using UserAccessManagement.Application.Users.DTOs;

namespace UserAccessManagement.Application.Users.Commands.CreateUser
{
    public record CreateUserCommand
     (
        string Name,
        string Lastname,
        string Email,
        string Password,
        string? Address
    ) : IRequest<ApiResponse<UserResponse>>;
}
