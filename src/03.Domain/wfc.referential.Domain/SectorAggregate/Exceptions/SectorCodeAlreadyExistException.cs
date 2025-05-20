using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.SectorAggregate.Exceptions;

public class SectorCodeAlreadyExistException : BusinessException
{
    public SectorCodeAlreadyExistException(string code) : base($"Sector with code {code} already exists.")
    {
    }
}