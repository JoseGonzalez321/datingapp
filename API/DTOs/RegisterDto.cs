using System;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class RegisterDto
{
    [Required]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [MinLength(4)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
}
