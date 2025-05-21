using BuildingBlocks.Core.Abstraction.CQRS;

namespace wfc.referential.Application.CountryIdentityDocs.Commands.DeleteCountryIdentityDoc;

public record DeleteCountryIdentityDocCommand(Guid CountryIdentityDocId) : ICommand<bool>;