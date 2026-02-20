using MediatR;
using UserAccessManagement.Application.Common;

namespace UserAccessManagement.Application.Users.Commands.DeleteUser
{
    public record DeleteUserCommand(Guid Id)
    : IRequest<ApiResponse<bool>>;
}
