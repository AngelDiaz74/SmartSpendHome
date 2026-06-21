using SmartSpendHome.Models;

namespace SmartSpendHome.Services;

public interface IReceiptParserService
{
    ParsedReceipt Parse(string ocrText);
}
