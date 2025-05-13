using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.ParamTypes.Commands.DeleteParamType;

public class DeleteParamTypeCommand : ICommand<Result<bool>>
{
    public Guid ParamTypeId { get; init; }
}
