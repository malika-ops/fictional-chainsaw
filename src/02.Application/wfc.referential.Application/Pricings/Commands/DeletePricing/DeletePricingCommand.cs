using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Pricings.Commands.DeletePricing;

public record DeletePricingCommand : ICommand<Result<bool>>
{
    public Guid PricingId { get; init; }
}