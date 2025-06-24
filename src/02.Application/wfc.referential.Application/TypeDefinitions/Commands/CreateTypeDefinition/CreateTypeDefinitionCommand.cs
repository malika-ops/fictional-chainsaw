using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;

namespace wfc.referential.Application.TypeDefinitions.Commands.CreateTypeDefinition;

public record CreateTypeDefinitionCommand : ICommand<Result<Guid>>
{
    public string Libelle { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}