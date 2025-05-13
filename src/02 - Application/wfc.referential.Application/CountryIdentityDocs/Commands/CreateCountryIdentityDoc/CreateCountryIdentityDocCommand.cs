using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.CountryIdentityDocs.Commands.CreateCountryIdentityDoc;

public class CreateCountryIdentityDocCommand : ICommand<Result<Guid>>
{
    public Guid CountryId { get; set; }
    public Guid IdentityDocumentId { get; set; }

    public CreateCountryIdentityDocCommand(Guid countryId, Guid identityDocumentId)
    {
        CountryId = countryId;
        IdentityDocumentId = identityDocumentId;
    }
}