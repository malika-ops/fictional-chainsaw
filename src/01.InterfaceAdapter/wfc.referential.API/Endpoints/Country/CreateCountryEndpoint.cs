using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Countries.Commands.CreateCountry;
using wfc.referential.Application.Countries.Dtos;


namespace wfc.referential.API.Endpoints.Country;

public class CreateCountryEndpoint(IMediator _mediator) : Endpoint<CreateCountryRequest, Guid>
{
    public override void Configure()
    {
        Post("/api/countries");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new Country";
            s.Description = "Creates a country with the provided number, name, code, ISO2, ISO3, dialing code, time zone, status and associated MonetaryZone. " +
                            "Required fields must be provided and valid.";
            s.Response<Guid>(200, "Returns the identifier of the newly created Country if successful");
            s.Response(400, "Returned if validation fails due to missing fields or violation of business rules");
            s.Response(500, "Returned if an unexpected error occurs on the server");
        });
        Options(o => o.WithTags(EndpointGroups.Countries));
    }

    public override async Task HandleAsync(CreateCountryRequest request, CancellationToken ct)
    {
        var command = request.Adapt<CreateCountryCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}
