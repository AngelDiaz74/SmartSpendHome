using System.Text.Json.Serialization;

namespace SmartSpendHome.Models;

public class GroceryReceiptItem
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("receipt_id")]
    public Guid ReceiptId { get; set; }

    [JsonPropertyName("item_name")]
    public string ItemName { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public decimal? Quantity { get; set; }

    [JsonPropertyName("price")]
    public decimal? Price { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}