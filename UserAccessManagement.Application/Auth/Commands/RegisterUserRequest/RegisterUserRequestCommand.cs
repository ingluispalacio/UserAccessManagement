using MediatR;
using UserAccessManagement.Application.Auth.DTOs;
using UserAccessManagement.Application.Common;

namespace UserAccessManagement.Application.Auth.Commands.RegisterUserRequest
{
    public record RegisterUserRequestCommand
     (
        string Name,
        string Lastname,
        string Email,
        string Password,
        string? Address
    ) : IRequest<ApiResponse<RegisterResponse>>;
}
