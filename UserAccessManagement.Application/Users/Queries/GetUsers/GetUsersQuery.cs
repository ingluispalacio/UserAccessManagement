using MediatR;
using UserAccessManagement.Application.Common;
using UserAccessManagement.Application.Users.DTOs;

namespace UserAccessManagement.Application.Users.Queries.GetUsers
{

    public record GetUsersQuery(
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<ApiResponse<PagedResult<UserResponse>>>;
}
