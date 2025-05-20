using BuildingBlocks.Core.Abstraction.CQRS;

namespace wfc.referential.Application.TypeDefinitions.Commands.UpdateTypeDefinition;

public record UpdateTypeDefinitionCommand(Guid TypeDefinitionId, string Libelle, string Description, bool IsEnabled) 
    : ICommand<Guid>;