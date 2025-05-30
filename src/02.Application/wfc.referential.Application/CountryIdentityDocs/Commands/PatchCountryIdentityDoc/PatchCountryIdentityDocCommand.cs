using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.CountryIdentityDocs.Commands.PatchCountryIdentityDoc;

public record PatchCountryIdentityDocCommand : ICommand<Result<bool>>
{
    public Guid CountryIdentityDocId { get; init; }
    public Guid? CountryId { get; init; }
    public Guid? IdentityDocumentId { get; init; }
    public bool? IsEnabled { get; init; }
}