using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.AgencyTierAggregate.Exceptions;

public class DuplicateAgencyTierCodeException : ConflictException
{
    public DuplicateAgencyTierCodeException(string code)
        : base($"AgencyTier with code '{code}' already exists for this agency & tier.") { }
}