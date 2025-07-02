using BuildingBlocks.Core.Abstraction.CQRS;
using wfc.referential.Application.TypeDefinitions.Dtos;

namespace wfc.referential.Application.TypeDefinitions.Queries.GetTypeDefinitionById;

public record GetTypeDefinitionByIdQuery : IQuery<GetTypeDefinitionsResponse>
{
    public Guid TypeDefinitionId { get; init; }
} 