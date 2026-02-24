namespace Groz_Backend.Models;

/// <summary>
/// Credențiale App Store Connect (API Key) pentru un utilizator.
/// Un user poate avea o singură setare (1:1).
/// </summary>
public class AppStoreConnectCredential
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>Issuer ID din App Store Connect.</summary>
    public string IssuerId { get; set; } = string.Empty;

    /// <summary>Key ID din App Store Connect.</summary>
    public string KeyId { get; set; } = string.Empty;

    /// <summary>Conținutul cheii private .p8 (text).</summary>
    public string PrivateKey { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
