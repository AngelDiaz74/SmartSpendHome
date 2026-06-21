namespace SmartSpendHome.Models;

public class AuthResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public SupabaseSession? Session { get; set; }
}