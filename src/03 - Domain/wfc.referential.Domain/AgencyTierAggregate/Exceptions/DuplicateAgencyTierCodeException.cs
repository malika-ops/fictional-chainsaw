using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.AgencyTierAggregate.Exceptions;

public class DuplicateAgencyTierCodeException : BusinessException
{
    public DuplicateAgencyTierCodeException(string code)
        : base($"AgencyTier with code '{code}' already exists for this agency & tier.") { }
}