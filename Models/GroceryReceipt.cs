using System.Text.Json.Serialization;

namespace SmartSpendHome.Models;

public class GroceryReceipt
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("user_id")]
    public Guid UserId { get; set; }

    [JsonPropertyName("budget_month_id")]
    public Guid BudgetMonthId { get; set; }

    [JsonPropertyName("receipt_date")]
    public DateTime ReceiptDate { get; set; } = DateTime.Today;

    [JsonPropertyName("store_name")]
    public string StoreName { get; set; } = string.Empty;

    [JsonPropertyName("total_amount")]
    public decimal TotalAmount { get; set; }

    [JsonPropertyName("image_path")]
    public string? ImagePath { get; set; }

    [JsonPropertyName("ocr_text")]
    public string? OcrText { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    public List<GroceryReceiptItem> Items { get; set; } = new();
}
