using MediatR;
using UserAccessManagement.Application.Common;

namespace UserAccessManagement.Application.Auth.Commands.ChangePassword
{
    public record ChangePasswordCommand(
        Guid UserId,
        string CurrentPassword,
        string NewPassword
    ) : IRequest<ApiResponse<Unit>>;
}
