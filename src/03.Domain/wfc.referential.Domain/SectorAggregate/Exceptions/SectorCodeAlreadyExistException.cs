using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.SectorAggregate.Exceptions;

public class SectorCodeAlreadyExistException(string code)
    : ConflictException($"Sector with code {code} already exists.");