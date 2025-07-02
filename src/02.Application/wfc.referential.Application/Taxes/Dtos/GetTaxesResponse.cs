namespace wfc.referential.Application.Taxes.Dtos;

public record GetTaxesResponse
{
    /// <summary>
    /// Unique identifier of the tax.
    /// </summary>
    /// <example>e2a1c3b4-5d6f-4a7b-8c9d-0e1f2a3b4c5d</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Unique code of the tax.
    /// </summary>
    /// <example>TAX001</example>
    public string Code { get; set; } = default!;

    /// <summary>
    /// Tax code in English.
    /// </summary>
    /// <example>VAT</example>
    public string CodeEn { get; set; } = default!;

    /// <summary>
    /// Tax code in Arabic.
    /// </summary>
    /// <example>ضريبة القيمة المضافة</example>
    public string CodeAr { get; set; } = default!;

    /// <summary>
    /// Description of the tax.
    /// </summary>
    /// <example>Value Added Tax</example>
    public string Description { get; set; } = default!;

    /// <summary>
    /// Fixed amount for the tax, if applicable.
    /// </summary>
    /// <example>10.00</example>
    public double FixedAmount { get; set; } = default!;

    /// <summary>
    /// Percentage or value of the tax.
    /// </summary>
    /// <example>0.2</example>
    public double Value { get; set; }

    /// <summary>
    /// Indicates whether the tax is enabled.
    /// </summary>
    /// <example>true</example>
    public bool IsEnabled { get; set; }
};
