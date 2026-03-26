using System.Text.Json.Serialization;

namespace Domain.DTOs;

public enum EventType
{
    [JsonPropertyName("INITIAL_PURCHASE")]
    InitialPurchase,
    [JsonPropertyName("NON_RENEWING_PURCHASE")]
    NonRenewingPurchase,
    [JsonPropertyName("RENEWAL")]
    Renewal
}

public class AddRevenuecatUserDto
{
    [JsonPropertyName("app_user_id")]
    public string AppUserId { get; set; } = string.Empty;
    
    [JsonPropertyName("country_code")]
    public string CountryCode { get; set; } = string.Empty;
    
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public EventType Type { get; set; }
    
    [JsonPropertyName("commission_percentage")]
    public double? CommissionPercentage { get; set; }
}