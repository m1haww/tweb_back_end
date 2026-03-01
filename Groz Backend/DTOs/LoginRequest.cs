using System.ComponentModel.DataAnnotations;

namespace Groz_Backend.DTOs;

/// <summary>
/// Ce trimite frontend-ul la login (POST /api/auth/login).
/// </summary>
public class LoginRequest
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}
