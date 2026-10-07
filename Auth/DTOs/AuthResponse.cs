namespace Auth.DTOs;

public record AuthResponse
{
    public required string Token  { get; set; }
    public DateTime ExpiresAt { get; init; }
    
    public required string Email { get; init; }

}