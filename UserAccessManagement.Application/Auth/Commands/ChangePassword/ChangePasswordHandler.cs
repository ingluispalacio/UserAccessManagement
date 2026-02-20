using MediatR;
using UserAccessManagement.Application.Common;
using UserAccessManagement.Application.Interfaces;
using UserAccessManagement.Domain.Users.Interfaces;

namespace UserAccessManagement.Application.Auth.Commands.ChangePassword
{
    public class ChangePasswordHandler
    : IRequestHandler<ChangePasswordCommand, ApiResponse<Unit>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;

        public ChangePasswordHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<Unit>> Handle(
            ChangePasswordCommand command,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(command.UserId);

            if (user is null)
                return ApiResponse<Unit>.Failure("Usuario no encontrado.");

            bool isValid = _passwordHasher.Verify(
                command.CurrentPassword,
                user.PasswordHash);

            if (!isValid)
                return ApiResponse<Unit>.Failure("Contraseña actual incorrecta.");

            string newHash = _passwordHasher.Hash(command.NewPassword);

            user.ChangePassword(newHash);

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<Unit>.Success(Unit.Value,
                "Contraseña actualizada correctamente.");
        }
    }

}
