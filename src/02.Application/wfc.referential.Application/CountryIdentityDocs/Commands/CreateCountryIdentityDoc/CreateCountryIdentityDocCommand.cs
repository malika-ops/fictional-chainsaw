using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.CountryIdentityDocs.Commands.CreateCountryIdentityDoc;

public record CreateCountryIdentityDocCommand(Guid CountryId, Guid IdentityDocumentId) 
    : ICommand<Result<Guid>>;