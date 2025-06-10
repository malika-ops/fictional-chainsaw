using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.CountryServices.Commands.UpdateCountryService;

public record UpdateCountryServiceCommand : ICommand<Result<bool>>
{
    public Guid CountryServiceId { get; set; }
    public Guid CountryId { get; set; }
    public Guid ServiceId { get; set; }
    public bool IsEnabled { get; set; } = true;
}