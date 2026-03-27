using System.Text.Json.Serialization;

namespace Domain.DTOs;

#region Enums

/// <summary>
/// RevenueCat webhook event type.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RevenueCatEventType
{
    TEST,
    INITIAL_PURCHASE,
    RENEWAL,
    CANCELLATION,
    UNCANCELLATION,
    NON_RENEWING_PURCHASE,
    SUBSCRIPTION_PAUSED,
    EXPIRATION,
    BILLING_ISSUE,
    PRODUCT_CHANGE,
    TRANSFER,
    SUBSCRIPTION_EXTENDED,
    TEMPORARY_ENTITLEMENT_GRANT,
    REFUND_REVERSED,
    INVOICE_ISSUANCE,
    VIRTUAL_CURRENCY_TRANSACTION,
    EXPERIMENT_ENROLLMENT,
    SUBSCRIBER_ALIAS
}

/// <summary>
/// Store the subscription belongs to.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RevenueCatStore
{
    AMAZON,
    APP_STORE,
    MAC_APP_STORE,
    PADDLE,
    PLAY_STORE,
    PROMOTIONAL,
    RC_BILLING,
    ROKU,
    STRIPE,
    TEST_STORE
}

/// <summary>
/// Store environment (SANDBOX or PRODUCTION).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RevenueCatEnvironment
{
    SANDBOX,
    PRODUCTION
}

/// <summary>
/// Period type of the transaction.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RevenueCatPeriodType
{
    TRIAL,
    INTRO,
    NORMAL,
    PROMOTIONAL,
    PREPAID
}

/// <summary>
/// Cancellation reason (CANCELLATION events) or expiration reason (EXPIRATION events).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RevenueCatCancelOrExpirationReason
{
    UNSUBSCRIBE,
    BILLING_ERROR,
    DEVELOPER_INITIATED,
    PRICE_INCREASE,
    CUSTOMER_SUPPORT,
    SUBSCRIPTION_PAUSED,
    UNKNOWN
}

#endregion

#region Subscriber & Experiment

/// <summary>
/// Subscriber attribute value. Map key is the attribute name (e.g. "$Favorite Cat").
/// </summary>
public class RevenueCatSubscriberAttributeDto
{
    [JsonPropertyName("updated_at_ms")]
    public long? UpdatedAtMs { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}

/// <summary>
/// Experiment enrollment. Present in experiments array and in EXPERIMENT_ENROLLMENT events.
/// </summary>
public class RevenueCatExperimentDto
{
    [JsonPropertyName("experiment_id")]
    public string? ExperimentId { get; set; }

    [JsonPropertyName("experiment_variant")]
    public string? ExperimentVariant { get; set; }

    [JsonPropertyName("enrolled_at_ms")]
    public long? EnrolledAtMs { get; set; }

    /// <summary>
    /// Offering id of the variant. For EXPERIMENT_ENROLLMENT events.
    /// </summary>
    [JsonPropertyName("offering_id")]
    public string? OfferingId { get; set; }
}

#endregion

#region Webhook Event & Payload

/// <summary>
/// Root payload of a RevenueCat webhook POST.
/// </summary>
public class RevenueCatWebhookPayloadDto
{
    [JsonPropertyName("api_version")]
    public string? ApiVersion { get; set; }

    [JsonPropertyName("event")]
    public RevenueCatEventDto? Event { get; set; }
}

/// <summary>
/// RevenueCat webhook event. Contains common fields and subscription lifecycle fields.
/// Not all fields are present on every event type; optional fields are nullable.
/// </summary>
public class RevenueCatEventDto
{
    // --- Common fields ---

    [JsonPropertyName("type")]
    public RevenueCatEventType Type { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("app_id")]
    public string? AppId { get; set; }

    [JsonPropertyName("event_timestamp_ms")]
    public long? EventTimestampMs { get; set; }

    /// <summary>
    /// Last seen app user id of the subscriber. Not present when type is TRANSFER.
    /// </summary>
    [JsonPropertyName("app_user_id")]
    public string? AppUserId { get; set; }

    /// <summary>
    /// The first app user id used by the subscriber.
    /// </summary>
    [JsonPropertyName("original_app_user_id")]
    public string? OriginalAppUserId { get; set; }

    [JsonPropertyName("aliases")]
    public List<string>? Aliases { get; set; }

    [JsonPropertyName("subscriber_attributes")]
    public Dictionary<string, RevenueCatSubscriberAttributeDto>? SubscriberAttributes { get; set; }

    [JsonPropertyName("experiments")]
    public List<RevenueCatExperimentDto>? Experiments { get; set; }

    // --- Subscription lifecycle fields ---

    [JsonPropertyName("product_id")]
    public string? ProductId { get; set; }

    [JsonPropertyName("entitlement_ids")]
    public List<string>? EntitlementIds { get; set; }

    /// <summary>
    /// Deprecated. Use EntitlementIds.
    /// </summary>
    [JsonPropertyName("entitlement_id")]
    public string? EntitlementId { get; set; }

    [JsonPropertyName("period_type")]
    public RevenueCatPeriodType? PeriodType { get; set; }

    [JsonPropertyName("purchased_at_ms")]
    public long? PurchasedAtMs { get; set; }

    /// <summary>
    /// Only for BILLING_ISSUE: when the grace period would expire.
    /// </summary>
    [JsonPropertyName("grace_period_expiration_at_ms")]
    public long? GracePeriodExpirationAtMs { get; set; }

    [JsonPropertyName("expiration_at_ms")]
    public long? ExpirationAtMs { get; set; }

    /// <summary>
    /// When an Android subscription would resume after being paused. SUBSCRIPTION_PAUSED, Play Store only.
    /// </summary>
    [JsonPropertyName("auto_resume_at_ms")]
    public long? AutoResumeAtMs { get; set; }

    [JsonPropertyName("store")]
    public RevenueCatStore? Store { get; set; }

    [JsonPropertyName("environment")]
    public RevenueCatEnvironment? Environment { get; set; }

    /// <summary>
    /// Only for RENEWAL events: whether the previous transaction was a free trial.
    /// </summary>
    [JsonPropertyName("is_trial_conversion")]
    public bool? IsTrialConversion { get; set; }

    /// <summary>
    /// Only for CANCELLATION events.
    /// </summary>
    [JsonPropertyName("cancel_reason")]
    public RevenueCatCancelOrExpirationReason? CancelReason { get; set; }

    /// <summary>
    /// Only for EXPIRATION events.
    /// </summary>
    [JsonPropertyName("expiration_reason")]
    public RevenueCatCancelOrExpirationReason? ExpirationReason { get; set; }

    /// <summary>
    /// New product after switch. Only for PRODUCT_CHANGE (e.g. DEFERRED replacement, App Store).
    /// </summary>
    [JsonPropertyName("new_product_id")]
    public string? NewProductId { get; set; }

    [JsonPropertyName("presented_offering_id")]
    public string? PresentedOfferingId { get; set; }

    [JsonPropertyName("price")]
    public double? Price { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("price_in_purchased_currency")]
    public double? PriceInPurchasedCurrency { get; set; }

    [JsonPropertyName("tax_percentage")]
    public double? TaxPercentage { get; set; }

    [JsonPropertyName("commission_percentage")]
    public double? CommissionPercentage { get; set; }

    [JsonPropertyName("takehome_percentage")]
    public double? TakehomePercentage { get; set; }

    [JsonPropertyName("transaction_id")]
    public string? TransactionId { get; set; }

    [JsonPropertyName("original_transaction_id")]
    public string? OriginalTransactionId { get; set; }

    [JsonPropertyName("is_family_share")]
    public bool? IsFamilyShare { get; set; }

    /// <summary>
    /// Only when type is TRANSFER: App User ID(s) transactions are taken from.
    /// </summary>
    [JsonPropertyName("transferred_from")]
    public List<string>? TransferredFrom { get; set; }

    /// <summary>
    /// Only when type is TRANSFER: App User ID(s) receiving the transactions.
    /// </summary>
    [JsonPropertyName("transferred_to")]
    public List<string>? TransferredTo { get; set; }

    [JsonPropertyName("country_code")]
    public string? CountryCode { get; set; }

    [JsonPropertyName("offer_code")]
    public string? OfferCode { get; set; }

    [JsonPropertyName("renewal_number")]
    public int? RenewalNumber { get; set; }
}

#endregion
