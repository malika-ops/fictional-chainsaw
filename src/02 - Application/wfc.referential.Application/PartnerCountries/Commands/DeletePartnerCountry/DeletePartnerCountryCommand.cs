using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.PartnerCountries.Commands.DeletePartnerCountry;

public record DeletePartnerCountryCommand(Guid PartnerCountryId)
    : ICommand<Result<bool>>;
