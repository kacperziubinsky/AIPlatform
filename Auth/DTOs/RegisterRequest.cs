using Auth.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Auth.DTOs;

public record RegisterRequest
{
    [Required, EmailAddress]
    public required string Email { get; init; }

    [Required, MinLength(8)]
    public required string Password { get; init; }

    [Required]
    public required Role Role { get; init; }
}