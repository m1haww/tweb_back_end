namespace Groz_Backend.DTOs;

/// <summary>
/// Ce primește frontend-ul după login/register: token-ul JWT și datele utilizatorului.
/// </summary>
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
}
