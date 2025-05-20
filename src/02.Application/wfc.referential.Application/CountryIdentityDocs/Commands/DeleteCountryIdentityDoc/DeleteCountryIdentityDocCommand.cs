using BuildingBlocks.Core.Abstraction.CQRS;

namespace wfc.referential.Application.CountryIdentityDocs.Commands.DeleteCountryIdentityDoc;

public class DeleteCountryIdentityDocCommand : ICommand<bool>
{
    public Guid CountryIdentityDocId { get; set; }

    public DeleteCountryIdentityDocCommand(Guid countryIdentityDocId)
    {
        CountryIdentityDocId = countryIdentityDocId;
    }
}