using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.TypeDefinitions.Dtos;


namespace wfc.referential.Application.TypeDefinitions.Queries.GetAllTypeDefinitions
{
    public class GetAllTypeDefinitionsQueryHandler : IQueryHandler<GetAllTypeDefinitionsQuery, PagedResult<GetAllTypeDefinitionsResponse>>
    {
        private readonly ITypeDefinitionRepository _typeDefinitionRepository;

        public GetAllTypeDefinitionsQueryHandler(ITypeDefinitionRepository typeDefinitionRepository)
        {
            _typeDefinitionRepository = typeDefinitionRepository;
        }

        public async Task<PagedResult<GetAllTypeDefinitionsResponse>> Handle(GetAllTypeDefinitionsQuery request, CancellationToken cancellationToken)
        {
            var typedefinitions = await _typeDefinitionRepository
                    .GetFilteredTypeDefinitionsAsync(request, cancellationToken);

            int totalCount = await _typeDefinitionRepository
                .GetCountTotalAsync(request, cancellationToken);

            var typedefinitionsResponse = typedefinitions.Adapt<List<GetAllTypeDefinitionsResponse>>();

            return new PagedResult<GetAllTypeDefinitionsResponse>(typedefinitionsResponse, totalCount, request.PageNumber, request.PageSize);

        }
    }
}
