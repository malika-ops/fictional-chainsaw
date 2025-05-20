using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Corridors.Commands.CreateCorridor;
using wfc.referential.Application.Corridors.Dtos;

namespace wfc.referential.API.Endpoints.Corridor;

public class CreateCorridor(IMediator _mediator) : Endpoint<CreateCorridorRequest, Guid>
{
    public override void Configure()
    {
        Post($"api/{EndpointGroups.Corridors}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = $"Create {nameof(Corridor)}";
            s.Response<PagedResult<CreateCorridorResponse>>(200, "Successful Response");
        });
        Options(o => o.WithTags(EndpointGroups.Corridors));
    }

    public override async Task HandleAsync(CreateCorridorRequest req, CancellationToken ct)
    {
        var corridorCommand = req.Adapt<CreateCorridorCommand>();
        var result = await _mediator.Send(corridorCommand, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}