using System.Text.Json.Serialization;

namespace SmartSpendHome.Models;

public class ShoppingProduct
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("user_id")]
    public Guid UserId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("default_quantity")]
    public string? DefaultQuantity { get; set; }

    [JsonPropertyName("is_pending")]
    public bool IsPending { get; set; } = true;

    [JsonPropertyName("needed_since")]
    public DateTime? NeededSince { get; set; }

    [JsonPropertyName("last_purchased_at")]
    public DateTime? LastPurchasedAt { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
