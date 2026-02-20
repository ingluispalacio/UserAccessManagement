using MediatR;
using UserAccessManagement.Application.Common;
using UserAccessManagement.Application.Users.DTOs;
using UserAccessManagement.Domain.Users.Interfaces;

namespace UserAccessManagement.Application.Users.Queries.GetUserByEmail
{
    public class GetUserByIdHandler 
        : IRequestHandler<GetUserByIdQuery, ApiResponse<UserResponse>>
    {
        private readonly IUserRepository _userRepository;

        public GetUserByIdHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ApiResponse<UserResponse>> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.id);

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
