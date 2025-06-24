namespace wfc.referential.Application.Taxes.Dtos;
/// <summary>
/// Represents the request parameters for retrieving a paginated list of taxes.
/// </summary>
/// <example>
/// {
///     PageNumber = 1,
///     PageSize = 10,
///     Code = "VAT",
///     CodeEn = "Value Added Tax",
///     CodeAr = "ضريبة القيمة المضافة",
///     Description = "General tax",
///     FixedAmount = 843.11,
///     Value = 15.0,
///     IsEnabled = true
/// }
/// </example>
public record GetFiltredTaxesRequest : FilterRequest
{
    /// <summary>Optional filter by tax code.</summary>
    public string? Code { get; init; }

    /// <summary>Optional filter by English tax code.</summary>
    public string? CodeEn { get; init; }

    /// <summary>Optional filter by Arabic tax code.</summary>
    public string? CodeAr { get; init; }

    /// <summary>Optional filter by tax description.</summary>
    public string? Description { get; init; }

    /// <summary>Optional filter by tax type.</summary>
    public double? FixedAmount { get; init; }

    /// <summary>Optional filter by tax value.</summary>
    public double? Value { get; init; }

}
