namespace wfc.referential.Application.Taxes.Dtos;
public record UpdateTaxRequest
{
    /// <summary>Tax Code.</summary>
    /// <example>TX002</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>Tax Code in English.</summary>
    /// <example>Service Tax</example>
    public string CodeEn { get; init; } = string.Empty;

    /// <summary>Tax Code in Arabic.</summary>
    /// <example>ضريبة الخدمات</example>
    public string CodeAr { get; init; } = string.Empty;

    /// <summary>Tax Description.</summary>
    /// <example>Applies to services provided within the country.</example>
    public string Description { get; init; } = string.Empty;

    /// <summary>Tax Type.</summary>
    /// <example>Service</example>
    public double FixedAmount { get; init; }

    /// <summary>Tax Value.</summary>
    /// <example>10.0</example>
    public double Rate { get; init; }

    /// <summary>Status of the tax.</summary>
    /// <example>Enabled</example>
    public bool IsEnabled { get; init; } = true;
}
