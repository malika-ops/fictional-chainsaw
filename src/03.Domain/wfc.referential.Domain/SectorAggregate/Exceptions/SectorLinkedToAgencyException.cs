using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.SectorAggregate.Exceptions;

public class SectorLinkedToAgencyException : BusinessException
{
    public SectorLinkedToAgencyException(Guid sectorId) : base($"Cannot delete sector with ID {sectorId} because it is linked to one or more agencies.")
    {
    }
}