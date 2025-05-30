using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.CountryIdentityDocs.Commands.UpdateCountryIdentityDoc;

public record UpdateCountryIdentityDocCommand : ICommand<Result<bool>>
{
    public Guid CountryIdentityDocId { get; set; }
    public Guid CountryId { get; set; }
    public Guid IdentityDocumentId { get; set; }
    public bool IsEnabled { get; set; } = true;
}