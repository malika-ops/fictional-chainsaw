using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.ParamTypes.Commands.DeleteParamType;

public record DeleteParamTypeCommand(Guid ParamTypeId) : ICommand<Result<bool>>;
