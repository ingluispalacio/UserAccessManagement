using MediatR;
using UserAccessManagement.Application.Auth.DTOs;
using UserAccessManagement.Application.Common;

namespace UserAccessManagement.Application.Auth.Commands.Login
{
    public record LoginCommand(
        string Email,
        string Password
    ) : IRequest<ApiResponse<LoginResponse>>;
}
