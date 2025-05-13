using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Partners.Dtos;
using wfc.referential.Application.Partners.Queries.GetAllPartners;

namespace wfc.referential.API.Endpoints.Partner;

public class GetAllPartners(IMediator _mediator) : Endpoint<GetAllPartnersRequest, PagedResult<PartnerResponse>>
{
    public override void Configure()
    {
        Get("/api/partners");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paginated list of Partners";
            s.Description = "Returns a paginated list of Partners. Supports optional filtering by code, label, network mode, payment mode, etc.";

            s.Response<PagedResult<PartnerResponse>>(200, "Paged list of Partners");
            s.Response(400, "If validation fails, e.g. invalid pageNumber/pageSize");
            s.Response(500, "Server error if unexpected");
        });

        Options(o => o.WithTags(EndpointGroups.Partners));
    }

    public override async Task HandleAsync(GetAllPartnersRequest requestObject, CancellationToken ct)
    {
        var query = requestObject.Adapt<GetAllPartnersQuery>();
        var result = await _mediator.Send(query, ct);
        await SendAsync(result, cancellation: ct);
    }
}