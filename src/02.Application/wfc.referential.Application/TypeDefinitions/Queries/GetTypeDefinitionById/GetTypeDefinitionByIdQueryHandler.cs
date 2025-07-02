using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Exceptions;
using Mapster;
using wfc.referential.Application.TypeDefinitions.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TypeDefinitionAggregate;

namespace wfc.referential.Application.TypeDefinitions.Queries.GetTypeDefinitionById;

public class GetTypeDefinitionByIdQueryHandler : IQueryHandler<GetTypeDefinitionByIdQuery, GetTypeDefinitionsResponse>
{
    private readonly ITypeDefinitionRepository _typeDefinitionRepository;

    public GetTypeDefinitionByIdQueryHandler(ITypeDefinitionRepository typeDefinitionRepository)
    {
        _typeDefinitionRepository = typeDefinitionRepository;
    }

    public async Task<GetTypeDefinitionsResponse> Handle(GetTypeDefinitionByIdQuery query, CancellationToken ct)
    {
        var id = TypeDefinitionId.Of(query.TypeDefinitionId);
        var entity = await _typeDefinitionRepository.GetByIdAsync(id, ct)
            ?? throw new ResourceNotFoundException($"TypeDefinition with id '{query.TypeDefinitionId}' not found.");

        return entity.Adapt<GetTypeDefinitionsResponse>();
    }
} 