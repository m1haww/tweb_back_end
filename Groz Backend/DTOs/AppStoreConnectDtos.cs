using System.ComponentModel.DataAnnotations;

namespace Groz_Backend.DTOs;

/// <summary>
/// Ce trimite frontend-ul când salvează credențialele App Store Connect.
/// </summary>
public class SaveAppStoreConnectRequest
{
    [Required(ErrorMessage = "Key ID is required.")]
    [MaxLength(100)]
    public string KeyId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Issuer ID is required.")]
    [MaxLength(100)]
    public string IssuerId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Private key (.p8) is required.")]
    public string PrivateKey { get; set; } = string.Empty;
}

/// <summary>
/// Răspuns la GET - nu expune niciodată PrivateKey.
/// </summary>
public class AppStoreConnectStatusResponse
{
    public bool Configured { get; set; }
    /// <summary>Ultimele 4 caractere ale Key ID pentru identificare (ex: "...XYZ1").</summary>
    public string? KeyIdSuffix { get; set; }
}
