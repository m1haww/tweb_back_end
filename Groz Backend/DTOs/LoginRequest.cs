using System.ComponentModel.DataAnnotations;

namespace Groz_Backend.DTOs;

/// <summary>
/// Ce trimite frontend-ul la login (POST /api/auth/login).
/// </summary>
public class LoginRequest
{
    [Required(ErrorMessage = "Email-ul este obligatoriu")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Parola este obligatorie")]
    public string Password { get; set; } = string.Empty;
}
