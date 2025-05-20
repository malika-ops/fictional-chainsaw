using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.TypeDefinitions.Commands.CreateTypeDefinition;

public record CreateTypeDefinitionCommand(string Libelle, string Description) 
    : ICommand<Result<Guid>>;