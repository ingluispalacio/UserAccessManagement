
using MediatR;
using UserAccessManagement.Application.Common;
using UserAccessManagement.Application.Interfaces;
using UserAccessManagement.Domain.Users.Interfaces;

namespace UserAccessManagement.Application.Users.Commands.DeleteUser
{
    public class DeleteUserCommandHandler
    : IRequestHandler<DeleteUserCommand, ApiResponse<bool>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteUserCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<bool>> Handle(
            DeleteUserCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.Id);

            if (user is null)
                return ApiResponse<bool>.Failure("Ususario no encontrado");

            user.SoftDelete();

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.Success(true, "Ususario eliminado exitosamente");
        }
    }

}
