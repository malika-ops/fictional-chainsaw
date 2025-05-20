using BuildingBlocks.Core.Exceptions;

namespace wfc.referential.Domain.SupportAccountAggregate.Exceptions;

public class SupportAccountAlreadyExistException : BusinessException
{
    public SupportAccountAlreadyExistException(string code) : base($"Support account with code {code} already exists.")
    {
    }
}