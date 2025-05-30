using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.CountryIdentityDocAggregate.Exceptions;

public class CountryIdentityDocAlreadyExistsException(Guid countryId, Guid identityDocumentId)
    : ConflictException($"Country identity document association with CountryId: {countryId} and IdentityDocumentId: {identityDocumentId} already exists.");