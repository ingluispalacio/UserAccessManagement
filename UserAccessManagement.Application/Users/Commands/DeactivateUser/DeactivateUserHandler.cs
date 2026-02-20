
using MediatR;
using UserAccessManagement.Application.Common;
using UserAccessManagement.Application.Interfaces;
using UserAccessManagement.Application.Users.Commands.DeleteUser;
using UserAccessManagement.Domain.Users.Interfaces;

namespace UserAccessManagement.Application.Users.Commands.DeactivateUser
{
    public class DeactivateUserCommandHandler
    : IRequestHandler<DeactivateUserCommand, ApiResponse<bool>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeactivateUserCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<bool>> Handle(
            DeactivateUserCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.Id);

            if (user is null)
                return ApiResponse<bool>.Failure("Ususario no encontrado");

            user.Deactivate();

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.Success(true, "Ususario desactivado exitosamente");
        }
    }

}
