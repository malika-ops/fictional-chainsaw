using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Pagination;
using Mapster;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.ParamTypes.Dtos;


namespace wfc.referential.Application.ParamTypes.Queries.GetAllParamTypes;

public class GetAllParamTypesQueryHandler : IQueryHandler<GetAllParamTypesQuery, PagedResult<GetAllParamTypesResponse>>
{
    private readonly IParamTypeRepository _paramTypeRepository;

    public GetAllParamTypesQueryHandler(IParamTypeRepository paramTypeRepository)
    {
        _paramTypeRepository = paramTypeRepository;
    }

    public async Task<PagedResult<GetAllParamTypesResponse>> Handle(GetAllParamTypesQuery request, CancellationToken cancellationToken)
    {

        var paramtypes = await _paramTypeRepository
        .GetFilteredParamTypesAsync(request, cancellationToken);

        int totalCount = await _paramTypeRepository
            .GetCountTotalAsync(request, cancellationToken);

        var paramtypesResponse = paramtypes.Adapt<List<GetAllParamTypesResponse>>();

        return new PagedResult<GetAllParamTypesResponse>(paramtypesResponse, totalCount, request.PageNumber, request.PageSize);

    }
}
