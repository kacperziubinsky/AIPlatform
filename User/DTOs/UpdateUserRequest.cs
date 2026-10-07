using System.ComponentModel.DataAnnotations;
using User.Models.Enums;

namespace User.DTOs;

public record UpdateUserRequest
{
    [MaxLength(50)]
    public string? Firstname { get; init; }

    [MaxLength(50)]
    public string? Lastname { get; init; }

    [Phone, MaxLength(20)]
    public string? Phone { get; init; }

    [MaxLength(100)]
    public string? City { get; init; }

    [MaxLength(30)]
    public List<string> Stack { get; init; } = new();

    [EnumDataType(typeof(Experience))]
    public Experience Experience { get; init; }
}