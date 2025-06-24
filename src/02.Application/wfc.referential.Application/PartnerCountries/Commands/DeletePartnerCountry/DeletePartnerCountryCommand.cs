using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.PartnerCountries.Commands.DeletePartnerCountry;

public record DeletePartnerCountryCommand : ICommand<Result<bool>>
{
    public Guid PartnerCountryId { get; init; }
}
