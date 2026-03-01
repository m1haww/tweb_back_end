using System.ComponentModel.DataAnnotations;

namespace Groz_Backend.DTOs;

/// <summary>
/// Ce trimite frontend-ul la înregistrare (POST /api/auth/register).
/// </summary>
public class RegisterRequest
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = string.Empty;

    public string? Name { get; set; }
}
