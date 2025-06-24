using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Pricings.Commands.PatchPricing;

public record PatchPricingCommand : ICommand<Result<bool>>
{
    public Guid PricingId { get; init; }
    public string? Code { get; init; }
    public string? Channel { get; init; }
    public decimal? MinimumAmount { get; init; }
    public decimal? MaximumAmount { get; init; }
    public decimal? FixedAmount { get; init; }
    public decimal? Rate { get; init; }
    public Guid? CorridorId { get; init; }
    public Guid? ServiceId { get; init; }
    public Guid? AffiliateId { get; init; }
    public bool? IsEnabled { get; init; }
}
