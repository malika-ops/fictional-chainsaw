using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.CountryIdentityDocs.Commands.DeleteCountryIdentityDoc;

public record DeleteCountryIdentityDocCommand : ICommand<Result<bool>>
{
    public Guid CountryIdentityDocId { get; init; }
}