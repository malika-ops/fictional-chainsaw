using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.CountryServices.Commands.DeleteCountryService;

public record DeleteCountryServiceCommand(Guid CountryServiceId) : ICommand<Result<bool>>;