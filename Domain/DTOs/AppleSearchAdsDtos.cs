using System.Text.Json.Serialization;

namespace Domain.DTOs;

public class AppleSearchAdsStatusResponse
{
    public bool Configured { get; set; }
    public string? KeyIdSuffix { get; set; }
}

public class AppleTokenEndpointResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public int? ExpiresIn { get; set; }
}

public class AppleSearchAdsAccessTokenResult
{
    public string AccessToken { get; set; } = string.Empty;
    public long? OrgId { get; set; }
    public string TokenType { get; set; } = "Bearer";
    public DateTime ExpiresAt { get; set; }
}

public class AppleSearchAdsAclDataDto
{
    [JsonPropertyName("orgName")]
    public string? OrgName { get; set; }

    [JsonPropertyName("orgId")]
    public long? OrgId { get; set; }
}

public class AppleSearchAdsAclResponseDto
{
    [JsonPropertyName("data")]
    public List<AppleSearchAdsAclDataDto>? Data { get; set; }
}
