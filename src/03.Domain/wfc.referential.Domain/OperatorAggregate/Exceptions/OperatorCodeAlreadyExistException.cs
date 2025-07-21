using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.OperatorAggregate.Exceptions;

public class OperatorCodeAlreadyExistException(string code)
    : ConflictException($"Operator with code {code} already exists.");