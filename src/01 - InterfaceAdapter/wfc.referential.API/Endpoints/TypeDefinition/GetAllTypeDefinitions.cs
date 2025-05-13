using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.TypeDefinitions.Dtos;
using wfc.referential.Application.TypeDefinitions.Queries.GetAllTypeDefinitions;

namespace wfc.referential.API.Endpoints.TypeDefinition;

public class GetAllTypeDefinitions(IMediator _mediator) : Endpoint<GetAllTypeDefinitionsRequest, PagedResult<GetAllTypeDefinitionsResponse>>
{
    public override void Configure()
    {
        Get("/api/typedefinitions");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated list of TypeDefinitions";
            s.Description = "Returns a paginated list of TypeDefinitions. Supports optional filtering by Libelle and description.";

            s.Response<PagedResult<GetAllTypeDefinitionsResponse>>(200, "Paged list of TypeDefinitions");
            s.Response(400, "If validation fails, e.g. invalid pageNumber/pageSize");
            s.Response(500, "Server error if unexpected");
        });

        Options(o => o.WithTags(EndpointGroups.TypeDefinitions));
    }

    public override async Task HandleAsync(GetAllTypeDefinitionsRequest requestObject, CancellationToken ct)
    {
        var query = requestObject.Adapt<GetAllTypeDefinitionsQuery>();

        var result = await _mediator.Send(query, ct);

        await SendAsync(result, cancellation: ct);
    }
}