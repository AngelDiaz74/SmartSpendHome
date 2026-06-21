using Blazored.LocalStorage;
using SmartSpendHome.Models;

namespace SmartSpendHome.Services;

public class GroceryBudgetService : IGroceryBudgetService
{
    private const string BudgetStorageKey = "smartspendhome_monthly_grocery_budget";
    private const string ReceiptsStorageKey = "smartspendhome_grocery_receipts";

    private readonly ILocalStorageService _localStorage;

    public GroceryBudgetService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task<GroceryBudgetSummary> GetSummaryAsync()
    {
        var monthlyBudget = await GetMonthlyBudgetAsync();
        var receipts = await GetReceiptsFromStorageAsync();

        var totalSpent = receipts.Sum(x => x.TotalAmount);

        return new GroceryBudgetSummary
        {
            MonthlyBudget = monthlyBudget,
            TotalSpent = totalSpent,
            ReceiptCount = receipts.Count
        };
    }

    public async Task<decimal> GetMonthlyBudgetAsync()
    {
        var budget = await _localStorage.GetItemAsync<decimal?>(BudgetStorageKey);

        return budget ?? 700;
    }

    public async Task SetMonthlyBudgetAsync(decimal amount)
    {
        if (amount < 0)
            amount = 0;

        await _localStorage.SetItemAsync(BudgetStorageKey, amount);
    }

    public async Task<List<GroceryReceipt>> GetReceiptsAsync()
    {
        var receipts = await GetReceiptsFromStorageAsync();

        return receipts
            .OrderByDescending(x => x.ReceiptDate)
            .ThenByDescending(x => x.CreatedAt)
            .ToList();
    }

    public async Task AddReceiptAsync(
        string storeName,
        decimal totalAmount,
        DateTime receiptDate,
        string? ocrText = null,
        List<ParsedReceiptItem>? detectedItems = null)
    {
        if (totalAmount <= 0)
            return;

        var receipts = await GetReceiptsFromStorageAsync();

        var receiptId = Guid.NewGuid();

        receipts.Add(new GroceryReceipt
        {
            Id = receiptId,
            StoreName = string.IsNullOrWhiteSpace(storeName)
                ? "Unknown Store"
                : storeName.Trim(),

            TotalAmount = totalAmount,
            ReceiptDate = receiptDate,
            OcrText = ocrText,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,

            Items = detectedItems?
                .Where(x => !string.IsNullOrWhiteSpace(x.ItemName))
                .Select(x => new GroceryReceiptItem
                {
                    Id = Guid.NewGuid(),
                    ReceiptId = receiptId,
                    ItemName = x.ItemName.Trim(),
                    Price = x.Price,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList() ?? new List<GroceryReceiptItem>()
        });

        await SaveReceiptsToStorageAsync(receipts);
    }

    public async Task DeleteReceiptAsync(Guid receiptId)
    {
        var receipts = await GetReceiptsFromStorageAsync();

        var receipt = receipts.FirstOrDefault(x => x.Id == receiptId);

        if (receipt is not null)
        {
            receipts.Remove(receipt);
            await SaveReceiptsToStorageAsync(receipts);
        }
    }

    private async Task<List<GroceryReceipt>> GetReceiptsFromStorageAsync()
    {
        var receipts = await _localStorage.GetItemAsync<List<GroceryReceipt>>(ReceiptsStorageKey);

        return receipts ?? new List<GroceryReceipt>();
    }

    private async Task SaveReceiptsToStorageAsync(List<GroceryReceipt> receipts)
    {
        await _localStorage.SetItemAsync(ReceiptsStorageKey, receipts);
    }
}