using System.Text.Json.Serialization;

namespace SmartSpendHome.Models;

public class GroceryBudgetMonth
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("user_id")]
    public Guid UserId { get; set; }

    [JsonPropertyName("month_start")]
    public DateTime MonthStart { get; set; }

    [JsonPropertyName("budget_amount")]
    public decimal BudgetAmount { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
}