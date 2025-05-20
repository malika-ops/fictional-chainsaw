using BuildingBlocks.Core.Abstraction.CQRS;

namespace wfc.referential.Application.CountryIdentityDocs.Commands.UpdateCountryIdentityDoc;

public class UpdateCountryIdentityDocCommand : ICommand<Guid>
{
    public Guid CountryIdentityDocId { get; set; }
    public Guid CountryId { get; set; }
    public Guid IdentityDocumentId { get; set; }
    public bool IsEnabled { get; set; }

    public UpdateCountryIdentityDocCommand(Guid countryIdentityDocId, Guid countryId, Guid identityDocumentId, bool isEnabled = true)
    {
        CountryIdentityDocId = countryIdentityDocId;
        CountryId = countryId;
        IdentityDocumentId = identityDocumentId;
        IsEnabled = isEnabled;
    }
}