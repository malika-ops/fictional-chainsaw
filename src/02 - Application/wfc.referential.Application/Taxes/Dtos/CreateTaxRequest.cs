namespace wfc.referential.Application.Taxes.Dtos;
public record CreateTaxRequest
{
    /// <summary>Tax Code.</summary>
    /// <example>TX001</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>Tax Code in English.</summary>
    /// <example>VAT</example>
    public string CodeEn { get; init; } = string.Empty;

    /// <summary>Tax Code in Arabic.</summary>
    /// <example>ضريبة القيمة المضافة</example>
    public string CodeAr { get; init; } = string.Empty;

    /// <summary>Tax Description.</summary>
    /// <example>Value Added Tax applicable to goods and services</example>
    public string Description { get; init; } = string.Empty;

    /// <summary>Tax Type.</summary>
    /// <example>Sales</example>
    public double FixedAmount { get; init; }

    /// <summary>Tax value.</summary>
    /// <example>15.0</example>
    public double Value { get; init; }

}
