using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Countries.Commands.UpdateCountry;
using wfc.referential.Application.Countries.Dtos;

namespace wfc.referential.API.Endpoints.Country;

public class UpdateCountryEndpoint(IMediator _mediator) : Endpoint<UpdateCountryRequest, Guid>
{
    public override void Configure()
    {
        Put("/api/countries/{CountryId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update an existing Country";
            s.Description = "Updates the country identified by CountryId with new fields.";
            s.Params["CountryId"] = "Country ID (GUID) from route";          

            s.Response<Guid>(200, "Returns the ID of the updated Country");
            s.Response(400, "Validation failed");
            s.Response(500, "Unexpected server error");
        });
        Options(o => o.WithTags(EndpointGroups.Countries));
    }

    public override async Task HandleAsync(UpdateCountryRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<UpdateCountryCommand>();        
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}
