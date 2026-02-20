using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserAccessManagement.Application.Common;
using UserAccessManagement.Application.Users.Commands.CreateUser;
using UserAccessManagement.Application.Users.Commands.DeleteUser;
using UserAccessManagement.Application.Users.Commands.UpdateUser;
using UserAccessManagement.Application.Users.DTOs;
using UserAccessManagement.Application.Users.Queries.GetUserByEmail;
using UserAccessManagement.Application.Users.Queries.GetUsers;

namespace UserAccessManagement.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateUserCommand command) 
    {
        ApiResponse<UserResponse> response = await _mediator.Send(command);
        return response.IsSuccess
                ? Ok(response)
                : BadRequest(response);
    }

    /// <summary>
    /// Get all users with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber <= 0 || pageSize <= 0)
            return BadRequest("PageNumber y PageSize deben ser mayores que cero.");

        ApiResponse<PagedResult<UserResponse>> result = await _mediator.Send(new GetUsersQuery(pageNumber, pageSize));
        return Ok(result);
    }

    /// <summary>
    /// Get user by Id
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        ApiResponse<UserResponse> user = await _mediator.Send(new GetUserByIdQuery(id));

        return user.IsSuccess
                ? Ok(user)
                : NotFound(user);
    }

    /// <summary>
    /// Get user by email
    /// </summary>
    [HttpGet("by-email/{email}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEmail(string email)
    {
        ApiResponse<UserResponse> user = await _mediator.Send(new GetUserByEmailQuery(email));

        return user.IsSuccess
                ? Ok(user)
                : NotFound(user);

    }

    /// <summary>
    /// Update user
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
    {

        UpdateUserCommand command 
            = new UpdateUserCommand(id,
                        request.Name,
                        request.Lastname,
                        request.Email,
                        request.Address);
        ApiResponse<Unit> response =  await _mediator.Send(command);
        return response.IsSuccess
            ? Ok(response)
            : BadRequest(response);
    }

    /// <summary>
    /// Soft delete user
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var response = await _mediator.Send(new DeleteUserCommand(id));

        return response.IsSuccess
            ? Ok(response)
            : NotFound(response);
    }


    /// <summary>
    /// Deactivate user
    /// </summary>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        ApiResponse<bool> response = await _mediator.Send(new DeactivateUserCommand(id));
        return response.IsSuccess
            ? Ok(response)
            : NotFound(response);
    }
}
