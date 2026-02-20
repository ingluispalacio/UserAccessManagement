namespace UserAccessManagement.Application.Auth.DTOs
{
    public record LoginDataRequest(
        string Email,
        string Password
    );

}
