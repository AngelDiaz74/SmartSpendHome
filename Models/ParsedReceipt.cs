namespace SmartSpendHome.Models;

public class ParsedReceipt
{
    public string? StoreName { get; set; }
    public decimal TotalAmount { get; set; }
    public List<ParsedReceiptItem> Items { get; set; } = new();
    public string RawText { get; set; } = string.Empty;
}
