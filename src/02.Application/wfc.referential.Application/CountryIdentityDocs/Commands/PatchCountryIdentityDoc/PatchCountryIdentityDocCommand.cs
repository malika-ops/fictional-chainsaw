using BuildingBlocks.Core.Abstraction.CQRS;

namespace wfc.referential.Application.CountryIdentityDocs.Commands.PatchCountryIdentityDoc;

public record PatchCountryIdentityDocCommand(Guid CountryIdentityDocId, Guid? CountryId, 
    Guid? IdentityDocumentId, bool? IsEnabled) : ICommand<Guid>;