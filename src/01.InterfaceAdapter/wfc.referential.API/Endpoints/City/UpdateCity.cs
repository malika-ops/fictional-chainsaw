using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Cities.Commands.UpdateCity;
using wfc.referential.Application.Cities.Dtos;

namespace wfc.referential.API.Endpoints.City;

public class UpdateCity(IMediator _mediator) : Endpoint<UpdateCityRequest, bool>
{
    public override void Configure()
    {
        Put("api/cities/{CityId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Fully update a City's properties";
            s.Description = "Updates all fields (code, name, status, countryId) of the specified City ID.";
            s.Params["CityId"] = "City ID (GUID) from route";

            s.Response<Guid>(200, "The ID of the updated City");
            s.Response(400, "If validation fails or the ID is invalid");
            s.Response(404, "City not found");
        });

        Options(o => o.WithTags(EndpointGroups.Cities));
    }

    public override async Task HandleAsync(UpdateCityRequest req, CancellationToken ct)
    {
        var command = req.Adapt<UpdateCityCommand>();

        var updatedId = await _mediator.Send(command, ct);

        await SendAsync(updatedId.Value, cancellation: ct);
    }
}
