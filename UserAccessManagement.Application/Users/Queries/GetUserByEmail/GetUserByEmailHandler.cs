using MediatR;
using UserAccessManagement.Application.Common;
using UserAccessManagement.Application.Users.DTOs;
using UserAccessManagement.Domain.Users.Interfaces;

namespace UserAccessManagement.Application.Users.Queries.GetUserByEmail
{
    public class GetUserByEmailHandler : IRequestHandler<GetUserByEmailQuery, ApiResponse<UserResponse>>
    {
        private readonly IUserRepository _userRepository;

        public GetUserByEmailHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ApiResponse<UserResponse>> Handle(
        GetUserByEmailQuery request,
        CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user is null)
                return ApiResponse<UserResponse>.Failure("Usuario no encontrado.");

            UserResponse response = new UserResponse(
                user.Id,
                user.Name,
                user.Lastname,
                user.Email.Value,
                user.Address ?? "NR",
                user.IsActive
            );

            return ApiResponse<UserResponse>.Success(response);
        }
    }
}
