using SmartSpendHome.Models;

namespace SmartSpendHome.Services;

public interface ISupabaseAuthService
{
    Task<AuthResult> SignInAsync(string email, string password);
    Task SignOutAsync();

    Task<bool> IsLoggedInAsync();
    Task<SupabaseSession?> GetSessionAsync();
    Task<Guid?> GetUserIdAsync();
    Task<string?> GetAccessTokenAsync();
}
