using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.AffiliateAggregate.Exceptions;

public class AffiliateLinkedToPricingException(Guid affiliateId)
    : ConflictException($"Cannot delete affiliate with ID {affiliateId} because it is linked to one or more pricings.");
