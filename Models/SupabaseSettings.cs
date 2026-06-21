namespace SmartSpendHome.Models;

public class SupabaseSettings
{
    public string Url { get; set; } = string.Empty;
    public string AnonKey { get; set; } = string.Empty;

    public string AuthUrl => $"{Url.TrimEnd('/')}/auth/v1";
    public string RestUrl => $"{Url.TrimEnd('/')}/rest/v1";
}
