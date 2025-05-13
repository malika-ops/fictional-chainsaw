using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Countries.Commands.DeleteCountry;
using wfc.referential.Application.Countries.Dtos;


namespace wfc.referential.API.Endpoints.Country;

public class DeleteCountryEndpoint(IMediator _mediator) : Endpoint<DeleteCountryRequest, bool>
{
    public override void Configure()
    {
        Delete("/api/countries/{CountryId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Delete a country by GUID";
            s.Description = "Deletes the Country identified by {CountryId} provided as route parameter.";
            s.Params["CountryId"] = "The GUID of the country to delete";

            s.Response<bool>(200, "True if deletion succeeded");
            s.Response(400, "Returned if deletion fails");
        });
        Options(o => o.WithTags(EndpointGroups.Countries));
    }

    public override async Task HandleAsync(DeleteCountryRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<DeleteCountryCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}
