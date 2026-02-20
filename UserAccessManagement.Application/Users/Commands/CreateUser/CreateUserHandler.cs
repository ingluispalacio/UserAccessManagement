using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using UserAccessManagement.Application.Common;
using UserAccessManagement.Application.Interfaces;
using UserAccessManagement.Application.Users.DTOs;
using UserAccessManagement.Domain.Users;
using UserAccessManagement.Domain.Users.Interfaces;
using UserAccessManagement.Domain.Users.ValueObjects;

namespace UserAccessManagement.Application.Users.Commands.CreateUser;

public sealed class CreateUserHandler
    : IRequestHandler<CreateUserCommand, ApiResponse<UserResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<CreateUserHandler> _logger;

    public CreateUserHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ILogger<CreateUserHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<ApiResponse<UserResponse>> Handle(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation(
                "Starting CreateUser for Email: {Email}",
                command.Email);

            Email emailResult;
            try
            {
                emailResult = Email.Create(command.Email);
            }
            catch (ArgumentException ex)
            {
                stopwatch.Stop();
                _logger.LogWarning(
                    "User creation failed. Invalid email format: {Email}. Error: {Error}. Duration: {Elapsed} ms",
                    command.Email,
                    ex.Message,
                    stopwatch.Elapsed.TotalMilliseconds);
                return ApiResponse<UserResponse>.Failure($"El formato del email no es valido.");
            }


            var existingUser = await _userRepository
                .GetByEmailAsync(emailResult.Value);

            if (existingUser != null)
            {
                stopwatch.Stop();
                _logger.LogWarning(
                    "User creation failed. Email already exists: {Email}. Duration: {Elapsed} ms",
                    command.Email,
                    stopwatch.Elapsed.TotalMilliseconds);
                return ApiResponse<UserResponse>.Failure($"El correo {command.Email} ya está en uso.");
            }
            

            string hashedPassword = _passwordHasher.Hash(command.Password);
           
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
            UserResponse response = new UserResponse(
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
            return ApiResponse<UserResponse>.Success(response, "Usuario registrado exitosamente en el sistema.");
            
        }
        catch (Exception ex) 
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Unexpected error while creating user {Email}. Duration: {Elapsed} ms",
                command.Email,
                stopwatch.Elapsed.TotalMilliseconds);
            return ApiResponse<UserResponse>.Failure($"Error inesperado. Detalle: {ex}");
        }
    }
}
