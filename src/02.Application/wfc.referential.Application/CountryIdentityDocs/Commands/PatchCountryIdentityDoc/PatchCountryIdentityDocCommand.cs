using BuildingBlocks.Core.Abstraction.CQRS;

namespace wfc.referential.Application.CountryIdentityDocs.Commands.PatchCountryIdentityDoc;

public class PatchCountryIdentityDocCommand : ICommand<Guid>
{
    // The ID from the route
    public Guid CountryIdentityDocId { get; }

    // The optional fields to update
    public Guid? CountryId { get; }
    public Guid? IdentityDocumentId { get; }
    public bool? IsEnabled { get; }

    public PatchCountryIdentityDocCommand(
        Guid countryIdentityDocId,
        Guid? countryId = null,
        Guid? identityDocumentId = null,
        bool? isEnabled = null)
    {
        CountryIdentityDocId = countryIdentityDocId;
        CountryId = countryId;
        IdentityDocumentId = identityDocumentId;
        IsEnabled = isEnabled;
    }
}