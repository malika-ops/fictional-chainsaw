using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Pricings.Commands.UpdatePricing;

public record UpdatePricingCommand : ICommand<Result<bool>>
{
    public Guid PricingId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Channel { get; init; } = string.Empty;
    public decimal MinimumAmount { get; init; }
    public decimal MaximumAmount { get; init; }
    public decimal? FixedAmount { get; init; }
    public decimal? Rate { get; init; }
    public Guid CorridorId { get; init; }
    public Guid ServiceId { get; init; }
    public Guid? AffiliateId { get; init; }
    public bool IsEnabled { get; init; } = true;
}