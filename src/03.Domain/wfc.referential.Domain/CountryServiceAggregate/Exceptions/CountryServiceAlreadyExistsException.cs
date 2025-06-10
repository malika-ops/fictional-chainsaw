using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.CountryServiceAggregate.Exceptions;

public class CountryServiceAlreadyExistsException(Guid countryId, Guid service)
    : ConflictException($"Association with CountryId: {countryId} and Service: {service} already exists.");