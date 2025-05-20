using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.TypeDefinitions.Commands.PatchTypeDefinition;

public record PatchTypeDefinitionCommand(Guid TypeDefinitionId, string? Libelle, string? Description, bool? IsEnabled) 
    : ICommand<Result<Guid>>;