namespace SmartSpendHome.Services;

public interface ISupabaseApiService
{
    Task<List<T>> GetListAsync<T>(string relativeUrl);
    Task<List<TResponse>> InsertAsync<TRequest, TResponse>(string relativeUrl, TRequest body);
    Task UpdateAsync<TRequest>(string relativeUrl, TRequest body);
    Task DeleteAsync(string relativeUrl);
}
