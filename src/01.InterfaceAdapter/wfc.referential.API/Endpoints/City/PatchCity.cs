using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Cities.Commands.PatchCity;
using wfc.referential.Application.Cities.Dtos;


namespace wfc.referential.API.Endpoints.City;

public class PatchCity(IMediator _mediator) : Endpoint<PatchCityRequest, bool>
{
    public override void Configure()
    {
        Patch("api/cities/{CityId}"); 
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Partially update a City properties";
            s.Description = "Updates only the provided fields (code, name, statu or countryId) of the specified City ID.";
            s.Params["CityId"] = "City ID (GUID) from route";

            s.Response<Guid>(200, "The ID of the updated City");
            s.Response(400, "If validation fails or the ID is invalid");
            s.Response(404, "City not found");
        });

        Options(o => o.WithTags(EndpointGroups.Cities));
    }

    public override async Task HandleAsync(PatchCityRequest req, CancellationToken ct)
    {
        var command = req.Adapt<PatchCityCommand>();

        var updatedId = await _mediator.Send(command, ct);

        await SendAsync(updatedId.Value, cancellation: ct);
    }
}
