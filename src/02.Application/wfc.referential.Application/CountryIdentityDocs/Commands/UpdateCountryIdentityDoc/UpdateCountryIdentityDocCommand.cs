using BuildingBlocks.Core.Abstraction.CQRS;

namespace wfc.referential.Application.CountryIdentityDocs.Commands.UpdateCountryIdentityDoc;

public record UpdateCountryIdentityDocCommand(Guid CountryIdentityDocId, Guid CountryId, Guid IdentityDocumentId, bool IsEnabled) 
    : ICommand<Guid>;