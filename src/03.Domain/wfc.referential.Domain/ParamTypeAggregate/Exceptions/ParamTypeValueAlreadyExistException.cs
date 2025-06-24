using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.ParamTypeAggregate.Exceptions;

public class ParamTypeValueAlreadyExistException(string value)
    : ConflictException($"ParamType with value '{value}' already exists.");