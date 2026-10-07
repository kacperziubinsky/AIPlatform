using Auth.Models.Enums;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; init; }
    public string Password { get; set; }
    public Role Role { get; init; }
    public bool Active { get; set; }
}