namespace wfc.referential.Application.Taxes.Dtos;
public record UpdateTaxRequest
{
    /// <summary>
    /// The string representation of the Tax's GUID (route param).
    /// </summary>
    /// <example>f3dcd2c0-a96e-4b18-8f67-4fd1f03dfabc</example>
    public Guid TaxId { get; init; }

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
    public double Value { get; init; }

    /// <summary>Status of the tax.</summary>
    /// <example>Enabled</example>
    public bool IsEnabled { get; init; } = true;
}
