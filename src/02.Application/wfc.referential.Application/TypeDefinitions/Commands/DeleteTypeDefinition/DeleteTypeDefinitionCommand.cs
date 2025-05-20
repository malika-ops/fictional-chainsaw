using BuildingBlocks.Core.Abstraction.CQRS;

namespace wfc.referential.Application.TypeDefinitions.Commands.DeleteTypeDefinition;

public record DeleteTypeDefinitionCommand(Guid TypeDefinitionId) 
    : ICommand<bool>;
