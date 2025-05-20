using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.ParamTypes.Dtos;
using wfc.referential.Application.ParamTypes.Queries.GetAllParamTypes;

namespace wfc.referential.API.Endpoints.ParamType;

public class GetAllParamTypes(IMediator _mediator) : Endpoint<GetAllParamTypesRequest, PagedResult<GetAllParamTypesResponse>>
{
    public override void Configure()
    {
        Get("/api/paramtypes");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated list of ParamTypes";
            s.Description = "Returns a paginated list of ParamTypes. Supports optional filtering by TypeDefinitionId, Value, and Status.";

            s.Response<PagedResult<GetAllParamTypesResponse>>(200, "Paged list of ParamTypes");
            s.Response(400, "If validation fails, e.g. invalid pageNumber/pageSize");
            s.Response(500, "Server error if unexpected");
        });

        Options(o => o.WithTags(EndpointGroups.ParamTypes));
    }

    public override async Task HandleAsync(GetAllParamTypesRequest requestObject, CancellationToken ct)
    {
        var query = requestObject.Adapt<GetAllParamTypesQuery>();
        var result = await _mediator.Send(query, ct);
        await SendAsync(result, cancellation: ct);
    }
}