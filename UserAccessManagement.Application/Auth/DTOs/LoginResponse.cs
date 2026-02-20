namespace UserAccessManagement.Application.Auth.DTOs
{
    public record LoginResponse(
        string Token,
        DateTime Expiration
    );

}
