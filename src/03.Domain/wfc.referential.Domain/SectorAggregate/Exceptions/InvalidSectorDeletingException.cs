using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.SectorAggregate.Exceptions;

public class InvalidSectorDeletingException : BusinessException
{
    public InvalidSectorDeletingException(string validationMessage) : base(validationMessage)
    {
    }
}