using Auth.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Auth.DTOs;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }
    [Required]
    [MinLength(8)]
    public required string Password { get; init; }
    public string? Firstname { get; init; }
    public string? Lastname { get; init; }
    public string? Phone { get; init; }
    public Role Role { get; init; }
}