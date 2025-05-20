using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Cities.Commands.DeleteCity;
using wfc.referential.Application.Cities.Dtos;

namespace wfc.referential.API.Endpoints.City;

public class DeleteCity(IMediator _mediator) : Endpoint<DeleteCityRequest, bool>
{
    public override void Configure()
    {
        Delete("api/cities/{CityId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Delete a city";
        });
        Options(o => o.WithTags(EndpointGroups.Cities));
    }

    public override async Task HandleAsync(DeleteCityRequest req, CancellationToken ct)
    {
        var command = req.Adapt<DeleteCityCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}