using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.PartnerCountries.Commands.CreatePartnerCountry;

public record CreatePartnerCountryCommand : ICommand<Result<Guid>>
{
    public Guid PartnerId { get; init; }
    public Guid CountryId { get; init; }
}