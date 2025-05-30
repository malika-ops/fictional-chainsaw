using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.CountryIdentityDocs.Commands.CreateCountryIdentityDoc;

public record CreateCountryIdentityDocCommand : ICommand<Result<Guid>>
{
    public Guid CountryId { get; init; }
    public Guid IdentityDocumentId { get; init; }
}