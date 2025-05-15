using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.PartnerCountries.Commands.UpdatePartnerCountry;

public record UpdatePartnerCountryCommand : ICommand<Result<Guid>>
{
    public Guid PartnerCountryId { get; init; }
    public Guid PartnerId { get; init; }
    public Guid CountryId { get; init; }
    public bool IsEnabled { get; init; }
}