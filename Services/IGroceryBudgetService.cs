using SmartSpendHome.Models;

namespace SmartSpendHome.Services;

public interface IGroceryBudgetService
{
    Task<GroceryBudgetSummary> GetSummaryAsync();
    Task<decimal> GetMonthlyBudgetAsync();
    Task SetMonthlyBudgetAsync(decimal amount);

    Task<List<GroceryReceipt>> GetReceiptsAsync();

    Task AddReceiptAsync(
        string storeName,
        decimal totalAmount,
        DateTime receiptDate,
        string? ocrText = null,
        List<ParsedReceiptItem>? detectedItems = null);

    Task DeleteReceiptAsync(Guid receiptId);
}
