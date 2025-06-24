namespace wfc.referential.Application.Pricings.Dtos;

public record PricingResponse
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Channel { get; init; } = string.Empty;
    public decimal MinimumAmount { get; init; }
    public decimal MaximumAmount { get; init; }
    public decimal? FixedAmount { get; init; }
    public decimal? Rate { get; init; }
    public bool IsEnabled { get; init; }
    public Guid CorridorId { get; init; }
    public Guid ServiceId { get; init; }
    public Guid? AffiliateId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastModified { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public string LastModifiedBy { get; init; } = string.Empty;

}
