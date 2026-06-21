namespace SmartSpendHome.Models;

public class ParsedReceiptItem
{
    public string ItemName { get; set; } = string.Empty;
    public decimal? Price { get; set; }
}
