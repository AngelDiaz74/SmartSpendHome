using System.Text.Json.Serialization;

namespace SmartSpendHome.Models;

public class SupabaseErrorResponse
{
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; set; }

    [JsonPropertyName("msg")]
    public string? Message { get; set; }
}
