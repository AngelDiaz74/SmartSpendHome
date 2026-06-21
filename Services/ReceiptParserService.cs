using SmartSpendHome.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SmartSpendHome.Services;

public class ReceiptParserService : IReceiptParserService
{
    public ParsedReceipt Parse(string ocrText)
    {
        var lines = ocrText
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        return new ParsedReceipt
        {
            StoreName = DetectStore(lines),
            TotalAmount = DetectTotal(lines),
            Items = DetectItems(lines),
            RawText = ocrText
        };
    }

    private string? DetectStore(List<string> lines)
    {
        if (lines.Count == 0)
            return null;

        var topLines = lines
            .Take(15)
            .Select(CleanStoreLine)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        foreach (var line in topLines)
        {
            var normalized = NormalizeForStoreMatch(line!);

            var detectedStore = DetectStoreFromOcrCorrections(normalized);

            if (!string.IsNullOrWhiteSpace(detectedStore))
                return detectedStore;
        }

        var bestFallback = topLines
            .Where(IsPotentialStoreName)
            .FirstOrDefault();

        return bestFallback;
    }
    private string NormalizeForStoreMatch(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var normalized = value.ToUpperInvariant();

        // Remove everything that is not A-Z or 0-9.
        // This handles OCR noise like: Sd | I'S club
        normalized = Regex.Replace(normalized, @"[^A-Z0-9]", "");

        return normalized;
    }

    private string? DetectStoreFromOcrCorrections(string normalized)
    {
        if (string.IsNullOrWhiteSpace(normalized))
            return null;

        // Sam's Club OCR variations
        if (normalized.Contains("SAMSCLUB") ||
            normalized.Contains("SAMCLUB") ||
            normalized.Contains("SAMS") ||
            normalized.Contains("SDISCLUB") ||
            normalized.Contains("DISCLUB") ||
            normalized.Contains("ISCLUB"))
        {
            return "Sam's Club";
        }

        // Walmart OCR variations
        if (normalized.Contains("WALMART") ||
            normalized.Contains("WALMARTSUPERCENTER") ||
            normalized.Contains("WALMARTNEIGHBORHOOD"))
        {
            return "Walmart";
        }

        // ALDI OCR variations
        if (normalized.Contains("ALDI") ||
            normalized.Contains("ALDIFOOD"))
        {
            return "ALDI";
        }

        // Publix OCR variations
        if (normalized.Contains("PUBLIX"))
        {
            return "Publix";
        }

        // Costco OCR variations
        if (normalized.Contains("COSTCO"))
        {
            return "Costco";
        }

        // Target OCR variations
        if (normalized.Contains("TARGET"))
        {
            return "Target";
        }

        return null;
    }
    private bool IsLikelySamsClub(string normalized)
    {
        if (string.IsNullOrWhiteSpace(normalized))
            return false;

        if (normalized.Contains("SAMSCLUB"))
            return true;

        if (normalized.Contains("SAMCLUB"))
            return true;

        if (normalized.Contains("SAMS"))
            return true;

        if (normalized.Contains("SAM"))
            return true;

        // Handles OCR result like: "Sd | I'S club" -> "SDISCLUB"
        if (normalized.Contains("CLUB") &&
            (normalized.Contains("SDIS") ||
             normalized.Contains("DISCLUB") ||
             normalized.Contains("ISCLUB") ||
             normalized.StartsWith("S") && normalized.EndsWith("CLUB")))
        {
            return true;
        }

        return false;
    }

    private bool IsPotentialStoreName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var line = value.Trim();
        var upper = line.ToUpperInvariant();

        if (line.Length < 3)
            return false;

        if (line.Length > 40)
            return false;

        var blockedKeywords = new[]
        {
        "TOTAL",
        "SUBTOTAL",
        "TAX",
        "CHANGE",
        "CASH",
        "VISA",
        "MASTERCARD",
        "DEBIT",
        "CREDIT",
        "APPROVAL",
        "AUTH",
        "CARD",
        "PAYMENT",
        "TENDER",
        "RECEIPT",
        "THANK",
        "WELCOME",
        "PHONE",
        "ADDRESS",
        "DATE",
        "TIME",
        "CASHIER",
        "TERMINAL",
        "STORE #",
        "ST#",
        "ITEM",
        "QTY",
        "PRICE",
        "SAVINGS"
    };

        if (blockedKeywords.Any(x => upper.Contains(x)))
            return false;

        if (Regex.IsMatch(line, @"\d{3}[-.\s]?\d{3}[-.\s]?\d{4}"))
            return false;

        if (Regex.IsMatch(line, @"\d+\.\d{2}"))
            return false;

        if (Regex.IsMatch(line, @"^\d+$"))
            return false;

        return true;
    }

    private decimal DetectTotal(List<string> lines)
    {
        var totalKeywords = new[]
        {
            "TOTAL",
            "AMOUNT DUE",
            "BALANCE",
            "SALE TOTAL",
            "GRAND TOTAL",
            "TOTAL DUE",
            "AMOUNT PAID"
        };

        foreach (var line in lines.AsEnumerable().Reverse())
        {
            var upper = line.ToUpperInvariant();

            if (!totalKeywords.Any(keyword => upper.Contains(keyword)))
                continue;

            var amount = ExtractLastMoneyValue(line);

            if (amount.HasValue && amount.Value > 0)
                return amount.Value;
        }

        return 0;
    }

    private List<ParsedReceiptItem> DetectItems(List<string> lines)
    {
        var items = new List<ParsedReceiptItem>();

        foreach (var line in lines)
        {
            if (IsNonItemLine(line))
                continue;

            var price = ExtractLastMoneyValue(line);

            if (!price.HasValue)
                continue;

            var itemName = RemoveLastMoneyValue(line);

            itemName = CleanItemName(itemName);

            if (string.IsNullOrWhiteSpace(itemName))
                continue;

            if (itemName.Length < 2)
                continue;

            items.Add(new ParsedReceiptItem
            {
                ItemName = itemName,
                Price = price.Value
            });
        }

        return items;
    }

    private decimal? ExtractLastMoneyValue(string line)
    {
        var matches = Regex.Matches(
            line,
            @"(?<!\d)(\d{1,4}[.,]\d{2})(?!\d)"
        );

        if (matches.Count == 0)
            return null;

        var value = matches[^1].Value.Replace(",", ".");

        return decimal.TryParse(
            value,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var amount)
                ? amount
                : null;
    }

    private string RemoveLastMoneyValue(string line)
    {
        return Regex.Replace(
            line,
            @"(?<!\d)(\d{1,4}[.,]\d{2})(?!\d)\s*$",
            "",
            RegexOptions.IgnoreCase
        ).Trim();
    }

    private bool IsNonItemLine(string line)
    {
        var upper = line.ToUpperInvariant();

        var blockedKeywords = new[]
        {
            "TOTAL",
            "SUBTOTAL",
            "TAX",
            "CHANGE",
            "CASH",
            "VISA",
            "MASTERCARD",
            "DISCOVER",
            "AMEX",
            "DEBIT",
            "CREDIT",
            "BALANCE",
            "APPROVAL",
            "AUTH",
            "CARD",
            "PAYMENT",
            "TENDER",
            "RECEIPT",
            "THANK",
            "WELCOME",
            "SAVINGS",
            "PHONE",
            "ADDRESS"
        };

        return blockedKeywords.Any(keyword => upper.Contains(keyword));
    }

    private string CleanItemName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var cleaned = value.Trim();

        cleaned = Regex.Replace(cleaned, @"\s+", " ");
        cleaned = Regex.Replace(cleaned, @"^[#*\-]+\s*", "");

        return cleaned.Trim();
    }

    private string? CleanStoreLine(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var cleaned = value.Trim();

        cleaned = Regex.Replace(cleaned, @"\s+", " ");
        cleaned = Regex.Replace(cleaned, @"^[^A-Za-z0-9]+", "");
        cleaned = Regex.Replace(cleaned, @"[^A-Za-z0-9\s#'&.-]+$", "");

        return cleaned.Trim();
    }

    private string NormalizeStoreName(string storeName)
    {
        return storeName.ToUpperInvariant() switch
        {
            "SAMS" => "Sam's Club",
            "SAM'S" => "Sam's Club",
            "ALDI" => "ALDI",
            "WALMART" => "Walmart",
            "PUBLIX" => "Publix",
            "TARGET" => "Target",
            "COSTCO" => "Costco",
            "KROGER" => "Kroger",
            "WHOLE FOODS" => "Whole Foods",
            "TRADER JOE" => "Trader Joe's",
            "WINN-DIXIE" => "Winn-Dixie",
            _ => storeName
        };
    }
}