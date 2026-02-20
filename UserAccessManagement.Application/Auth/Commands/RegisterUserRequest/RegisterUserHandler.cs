using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using UserAccessManagement.Application.Common;
using UserAccessManagement.Application.Interfaces;
using UserAccessManagement.Application.Auth.DTOs;
using UserAccessManagement.Domain.Users;
using UserAccessManagement.Domain.Users.Interfaces;
using UserAccessManagement.Domain.Users.ValueObjects;
using UserAccessManagement.Application.Auth.Commands.RegisterUserRequest;

namespace UserAccessManagement.Application.Auth.Commands.RegisterUser;

public sealed class RegisterUserHandler
    : IRequestHandler<RegisterUserRequestCommand, ApiResponse<RegisterResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<RegisterUserHandler> _logger;

    public RegisterUserHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ILogger<RegisterUserHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<ApiResponse<RegisterResponse>> Handle(
        RegisterUserRequestCommand command,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation(
                "Starting CreateUser for Email: {Email}",
                command.Email);


            var existingUser = await _userRepository
                .GetByEmailAsync(command.Email);

            if (existingUser != null)
            {
                stopwatch.Stop();
                _logger.LogWarning(
                    "User creation failed. Email already exists: {Email}. Duration: {Elapsed} ms",
                    command.Email,
                    stopwatch.Elapsed.TotalMilliseconds);
                return ApiResponse<RegisterResponse>.Failure($"El correo {command.Email} ya está en uso.");
            }
            

            string hashedPassword = _passwordHasher.Hash(command.Password);
            Email emailResult = Email.Create(command.Email);
            string address = command.Address ?? "NR";
            User user = new User(
                command.Name,
                command.Lastname,
                address,
                emailResult,
                hashedPassword
            );

            await _userRepository.AddAsync(user);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            RegisterResponse response = new RegisterResponse(
                user.Id,
                user.Name,
                user.Lastname,
                user.Email.Value,
                user.Address ?? address,
                user.IsActive
             ); 

            stopwatch.Stop();


            _logger.LogInformation("CreateUserHandler executed in {Elapsed} ms",
                stopwatch.Elapsed.TotalMilliseconds);
            return ApiResponse<RegisterResponse>.Success(response, "Usuario registrado exitosamente en el sistema.");
            
        }
        catch (Exception ex) 
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Unexpected error while creating user {Email}. Duration: {Elapsed} ms",
                command.Email,
                stopwatch.Elapsed.TotalMilliseconds);
            return ApiResponse<RegisterResponse>.Failure($"Error inesperado. Detalle: {ex}");
        }
    }
}
