using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Cities.Commands.CreateCity;
using wfc.referential.Application.Cities.Dtos;

namespace wfc.referential.API.Endpoints.City;

public class CreateCity(IMediator _mediator) : Endpoint<CreateCityRequest, Guid>
{
    public override void Configure()
    {
        Post("api/cities");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create city";
            s.Response<Guid>(200, "Successful Response");
        });
        Options(o => o.WithTags(EndpointGroups.Cities));
    }

    public override async Task HandleAsync(CreateCityRequest req, CancellationToken ct)
    {
        var cityCommand = req.Adapt<CreateCityCommand>();
        var result = await _mediator.Send(cityCommand, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}
