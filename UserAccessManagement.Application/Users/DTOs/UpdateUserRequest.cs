namespace UserAccessManagement.Application.Users.DTOs
{
    public record UpdateUserRequest(
        string Name,
        string Lastname,
        string Email,
        string? Address
    );

}
