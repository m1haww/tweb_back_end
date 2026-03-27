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

public class SubscriberAttributes
{
    [JsonPropertyName("mediaSource")]
    public long MediaSource { get; set; }
    
    [JsonPropertyName("inst")]
    public DateTime? ClickDate { get; set; }
    
    [JsonPropertyName("orgId")]
    public long? OrgId { get; set; }
    
    [JsonPropertyName("campaignId")]
    public long? CampaignId { get; set; }
    
    [JsonPropertyName("keywordId")]
    public long? KeywordId { get; set; }
    
    [JsonPropertyName("adGroupId")]
    public long? AdGroupId { get; set; }
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
    
    [JsonPropertyName("subscriber_attributes")]
    public SubscriberAttributes? SubscriberAttributes { get; set; } 
}