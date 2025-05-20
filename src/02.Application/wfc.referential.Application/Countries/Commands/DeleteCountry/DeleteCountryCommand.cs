using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.Countries.Commands.DeleteCountry;

public record DeleteCountryCommand : ICommand<Result<bool>>
{
    public Guid CountryId { get; init; }

}
