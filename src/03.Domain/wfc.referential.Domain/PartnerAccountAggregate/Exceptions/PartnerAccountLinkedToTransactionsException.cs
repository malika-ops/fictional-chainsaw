using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.PartnerAccountAggregate.Exceptions;

public class PartnerAccountLinkedToTransactionsException(Guid partnerAccountId)
    : ConflictException($"Cannot delete partner account with ID {partnerAccountId} because it is linked to one or more transactions.");