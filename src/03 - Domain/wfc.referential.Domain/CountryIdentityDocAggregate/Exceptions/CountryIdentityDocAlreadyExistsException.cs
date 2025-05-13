using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.CountryIdentityDocAggregate.Exceptions;

public class CountryIdentityDocAlreadyExistsException : BusinessException
{
    public CountryIdentityDocAlreadyExistsException(Guid countryId, Guid identityDocumentId)
        : base($"Country identity document association with CountryId: {countryId} and IdentityDocumentId: {identityDocumentId} already exists.")
    {
    }
}