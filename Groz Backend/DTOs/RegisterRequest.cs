using System.ComponentModel.DataAnnotations;

namespace Groz_Backend.DTOs;

/// <summary>
/// Ce trimite frontend-ul la înregistrare (POST /api/auth/register).
/// </summary>
public class RegisterRequest
{
    [Required(ErrorMessage = "Email-ul este obligatoriu")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Parola este obligatorie")]
    [MinLength(6, ErrorMessage = "Parola trebuie să aibă minim 6 caractere")]
    public string Password { get; set; } = string.Empty;

    public string? Name { get; set; }
}
