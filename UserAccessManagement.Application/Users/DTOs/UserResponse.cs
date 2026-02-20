namespace UserAccessManagement.Application.Users.DTOs
{
    public record UserResponse(
        Guid Id,
        string Name,
        string Lastname,
        string Email,
        string Address,
        bool IsActive
    );
}
