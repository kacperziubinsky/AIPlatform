namespace Auth.DTOs;

public class AuthResponse
{
    public required string Token  { get; set; }
    public DateTime ExpiresAt { get; init; }
    
    public required string Email { get; init; }

}