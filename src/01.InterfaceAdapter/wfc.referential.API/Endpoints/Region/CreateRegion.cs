using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Regions.Commands.CreateRegion;
using wfc.referential.Application.Regions.Dtos;

namespace wfc.referential.API.Endpoints.Region;

public class CreateRegion(IMediator _mediator) : Endpoint<CreateRegionRequest, Guid>
{
    public override void Configure()
    {
        Post("api/regions");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create region";
            s.Response<PagedResult<CreateRegionResponse>>(200, "Successful Response");
        });
        Options(o => o.WithTags(EndpointGroups.Regions));
    }

    public override async Task HandleAsync(CreateRegionRequest req, CancellationToken ct)
    {
        var regionCommand = req.Adapt<CreateRegionCommand>();
        var result = await _mediator.Send(regionCommand, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}