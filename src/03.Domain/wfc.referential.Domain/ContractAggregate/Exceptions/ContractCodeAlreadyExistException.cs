using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.ContractAggregate.Exceptions;

public class ContractCodeAlreadyExistException(string code)
    : ConflictException($"Contract with code {code} already exists.");