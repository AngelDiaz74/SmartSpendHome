namespace SmartSpendHome.Models;

public class GroceryBudgetSummary
{
    public decimal MonthlyBudget { get; set; }
    public decimal TotalSpent { get; set; }

    public decimal RemainingBudget => MonthlyBudget - TotalSpent;

    public int ReceiptCount { get; set; }

    public bool IsOverBudget => RemainingBudget < 0;
}
