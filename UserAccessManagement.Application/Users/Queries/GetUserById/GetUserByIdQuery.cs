using MediatR;
using UserAccessManagement.Application.Common;
using UserAccessManagement.Application.Users.DTOs;

namespace UserAccessManagement.Application.Users.Queries.GetUserByEmail
{
    public record GetUserByIdQuery(Guid id) : IRequest<ApiResponse<UserResponse>>;
}

