using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.CountryServices.Commands.PatchCountryService;

public record PatchCountryServiceCommand : ICommand<Result<bool>>
{
    public Guid CountryServiceId { get; init; }
    public Guid? CountryId { get; init; }
    public Guid? ServiceId { get; init; }
    public bool? IsEnabled { get; init; }
}