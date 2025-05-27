using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.SectorAggregate.Exceptions;

public class SectorLinkedToAgencyException(Guid sectorId)
    : BusinessException($"Cannot delete sector with ID {sectorId} because it is linked to one or more agencies.");