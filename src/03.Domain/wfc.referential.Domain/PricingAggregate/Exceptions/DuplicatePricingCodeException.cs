using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.PricingAggregate.Exceptions;

public class DuplicatePricingCodeException : ConflictException
{
    public DuplicatePricingCodeException(string code)
        : base($"Pricing with code '{code}' already exists.") { }
}