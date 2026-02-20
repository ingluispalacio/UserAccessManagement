using MediatR;
using System.Net;
using UserAccessManagement.Application.Common;
using UserAccessManagement.Application.Interfaces;
using UserAccessManagement.Domain.Users.Interfaces;
using UserAccessManagement.Domain.Users.ValueObjects;

namespace UserAccessManagement.Application.Users.Commands.UpdateUser;

public class UpdateUserHandler
    : IRequestHandler<UpdateUserCommand, ApiResponse<Unit>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<Unit>> Handle(
        UpdateUserCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.Id);

        if (user is null)
            return ApiResponse<Unit>.Failure("Usuario no encontrado.");


        Email emailResult = Email.Create(command.Email);
        string address = command.Address ?? "NR";

        user.Update(
            command.Name,
            command.Lastname,
            address,
            emailResult
        );

        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<Unit>.Success(Unit.Value, "Usuario actualizado exitosamente.");
    }
}
