namespace wfc.referential.Application.Taxes.Dtos;

public record PatchTaxRequest
{
    /// <summary>If provided, updates the tax code. If omitted, the code remains unchanged.</summary>
    public string? Code { get; init; }

    /// <summary>If provided, updates the English tax code. If omitted, remains unchanged.</summary>
    public string? CodeEn { get; init; }

    /// <summary>If provided, updates the Arabic tax code. If omitted, remains unchanged.</summary>
    public string? CodeAr { get; init; }

    /// <summary>If provided, updates the tax description. If omitted, remains unchanged.</summary>
    public string? Description { get; init; }

    /// <summary>If provided, updates the tax type. If omitted, remains unchanged.</summary>
    public double? FixedAmount { get; init; }

    /// <summary>If provided, updates the tax value. If omitted, remains unchanged.</summary>
    public double? Rate { get; init; }

    /// <summary>If provided, updates the tax status. If omitted, remains unchanged.</summary>
    public bool? IsEnabled { get; set; }
}
