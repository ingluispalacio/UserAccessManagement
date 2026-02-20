using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserAccessManagement.Application.Auth.Commands.ChangePassword;
using UserAccessManagement.Application.Auth.Commands.Login;
using UserAccessManagement.Application.Auth.Commands.RegisterUserRequest;
using UserAccessManagement.Application.Auth.DTOs;
using UserAccessManagement.Application.Common;

namespace UserAccessManagement.Api.Controllers.Users
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserRequestCommand command)
        {
            ApiResponse<RegisterResponse> response = await _mediator.Send(command);

            return response.IsSuccess
                ? Ok(response)
                : BadRequest(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDataRequest request)
        {
            var command = new LoginCommand(
                request.Email,
                request.Password);

            var response = await _mediator.Send(command);

            return response.IsSuccess ? Ok(response) : Unauthorized(response);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId is null)
                return Unauthorized();

            var command = new ChangePasswordCommand(
                Guid.Parse(userId),
                request.CurrentPassword,
                request.NewPassword);

            var response = await _mediator.Send(command);

            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

    }
}
