namespace wfc.referential.Application.Taxes.Dtos;

public record GetTaxesResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public string CodeEn{ get; set; } = default!;
    public string CodeAr { get; set; } = default!;
    public string Description { get; set; } = default!;
    public double FixedAmount { get; set; } = default!;
    public double Value{ get; set; }
    public bool IsEnabled { get; set; }
};
