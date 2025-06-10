using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.CountryServices.Commands.CreateCountryService;

public record CreateCountryServiceCommand : ICommand<Result<Guid>>
{
    public Guid CountryId { get; init; }
    public Guid ServiceId { get; init; }
}