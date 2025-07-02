using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.ContractDetailsAggregate.Exceptions;

public class ContractDetailsNotFoundException(Guid contractDetailsId)
    : ResourceNotFoundException($"ContractDetails with ID {contractDetailsId} was not found.");