using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Pricings.Commands.CreatePricing;

public record CreatePricingCommand : ICommand<Result<Guid>>
{
    public string Code { get; init; } = string.Empty;
    public string Channel { get; init; } = string.Empty;
    public decimal MinimumAmount { get; init; }
    public decimal MaximumAmount { get; init; }
    public decimal? FixedAmount { get; init; }
    public decimal? Rate { get; init; }
    public Guid CorridorId { get; init; }
    public Guid ServiceId { get; init; }
    public Guid? AffiliateId { get; init; }
}
