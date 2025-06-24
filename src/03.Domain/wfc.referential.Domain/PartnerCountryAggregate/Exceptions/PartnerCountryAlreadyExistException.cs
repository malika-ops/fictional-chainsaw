using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.PartnerCountryAggregate.Exceptions;

public class PartnerCountryAlreadyExistException : ConflictException
{
    public PartnerCountryAlreadyExistException(Guid partnerId, Guid countryId)
        : base($"Partner ({partnerId}) is already linked to Country ({countryId}).")
    { }
}