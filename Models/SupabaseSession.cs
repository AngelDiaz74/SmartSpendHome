namespace SmartSpendHome.Models;

public class SupabaseSession
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "bearer";
    public int ExpiresIn { get; set; }

    public Guid UserId { get; set; }
    public string? Email { get; set; }
}