using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.Partners.Dtos;

namespace wfc.referential.Application.Partners.Commands.GetPricingConfiguration;

public record GetPricingConfigurationCommand : ICommand<GetPricingConfigurationResponse>
{
    public Guid PartnerId { get; init; }
    public Guid ServiceId { get; init; }
    public Guid CorridorId { get; init; }
    public Guid AffiliateId { get; init; }
    public string Channel { get; init; } = string.Empty;
    public decimal Amount { get; init; }
}