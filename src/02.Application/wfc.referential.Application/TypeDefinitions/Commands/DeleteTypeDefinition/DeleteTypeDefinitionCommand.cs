using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.TypeDefinitions.Commands.DeleteTypeDefinition;

public record DeleteTypeDefinitionCommand : ICommand<Result<bool>>
{
    public Guid TypeDefinitionId { get; }
    public DeleteTypeDefinitionCommand(Guid typeDefinitionId) => TypeDefinitionId = typeDefinitionId;
}