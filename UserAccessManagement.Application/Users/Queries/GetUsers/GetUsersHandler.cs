using MediatR;
using UserAccessManagement.Application.Common;
using UserAccessManagement.Application.Users.DTOs;
using UserAccessManagement.Domain.Users.Interfaces;

namespace UserAccessManagement.Application.Users.Queries.GetUsers
{
    public class GetUsersHandler
    : IRequestHandler<GetUsersQuery, ApiResponse<PagedResult<UserResponse>>>
    {
        private readonly IUserRepository _userRepository;

        public GetUsersHandler(IUserRepository repository)
        {
            _userRepository = repository;
        }

        public async Task<ApiResponse<PagedResult<UserResponse>>> Handle(
            GetUsersQuery request,
            CancellationToken cancellationToken)
        {
            var users = await _userRepository.GetAllAsync(request.PageNumber, request.PageSize);
            var total = await _userRepository.CountAsync();

            var result = new PagedResult<UserResponse>
            {
                Items = users.Select(u => new UserResponse(
                    u.Id,
                    u.Name,
                    u.Lastname,
                    u.Email.Value,
                    u.Address ?? "NR",
                    u.IsActive
                )),
                TotalCount = total,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return ApiResponse<PagedResult<UserResponse>>.Success(result);
        }
    }
}
