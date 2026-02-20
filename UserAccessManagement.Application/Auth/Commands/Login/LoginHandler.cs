using MediatR;
using Microsoft.Extensions.Logging;
using UserAccessManagement.Application.Auth.DTOs;
using UserAccessManagement.Application.Common;
using UserAccessManagement.Application.Interfaces;
using UserAccessManagement.Domain.Users.Interfaces;

namespace UserAccessManagement.Application.Auth.Commands.Login
{
    public class LoginHandler
    : IRequestHandler<LoginCommand, ApiResponse<LoginResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly ILogger<LoginHandler> _logger;

        public LoginHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator,
            ILogger<LoginHandler> logger)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
            _logger = logger;
        }

        public async Task<ApiResponse<LoginResponse>> Handle(
            LoginCommand command,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(command.Email);

            if (user is null)
                return ApiResponse<LoginResponse>.Failure("Credenciales inválidas.");

            bool isValid = _passwordHasher.Verify(
                command.Password,
                user.PasswordHash);

            if (!isValid)
                return ApiResponse<LoginResponse>.Failure("Credenciales inválidas.");

            if (!user.IsActive)
                return ApiResponse<LoginResponse>.Failure("Usuario desactivado.");

            string token = _jwtTokenGenerator.GenerateToken(user);

            var response = new LoginResponse(
                token,
                DateTime.UtcNow.AddHours(2)
            );
            _logger.LogInformation("Usuario {UserId} ha iniciado sesión", user.Id);
            return ApiResponse<LoginResponse>.Success(response, "Login exitoso.");
        }
    }

}
