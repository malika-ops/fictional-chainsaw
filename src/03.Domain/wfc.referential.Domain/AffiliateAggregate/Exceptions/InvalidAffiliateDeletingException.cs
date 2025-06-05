using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.AffiliateAggregate.Exceptions;

public class InvalidAffiliateDeletingException(string validationMessage)
    : ResourceNotFoundException(validationMessage);