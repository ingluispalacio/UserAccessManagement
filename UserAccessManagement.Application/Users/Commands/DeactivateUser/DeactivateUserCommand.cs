using MediatR;
using UserAccessManagement.Application.Common;

namespace UserAccessManagement.Application.Users.Commands.DeleteUser
{
    public record DeactivateUserCommand(Guid Id)
    : IRequest<ApiResponse<bool>>;
}
