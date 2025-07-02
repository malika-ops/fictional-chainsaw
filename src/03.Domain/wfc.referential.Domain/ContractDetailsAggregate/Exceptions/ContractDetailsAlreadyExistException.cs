using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.ContractDetailsAggregate.Exceptions;

public class ContractDetailsAlreadyExistException(Guid contractId, Guid pricingId)
    : ConflictException($"ContractDetails with ContractId {contractId} and PricingId {pricingId} already exists.");