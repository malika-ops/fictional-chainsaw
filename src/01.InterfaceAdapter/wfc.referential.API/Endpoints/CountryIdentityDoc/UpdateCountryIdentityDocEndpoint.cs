using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.CountryIdentityDocs.Commands.UpdateCountryIdentityDoc;
using wfc.referential.Application.CountryIdentityDocs.Dtos;

namespace wfc.referential.API.Endpoints.CountryIdentityDoc;

public class UpdateCountryIdentityDocEndpoint(IMediator _mediator)
    : Endpoint<UpdateCountryIdentityDocRequest, bool>
{
    public override void Configure()
    {
        Put("/api/countryidentitydocs/{CountryIdentityDocId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update an existing Country-Identity Document association";
            s.Description = "Updates the association identified by {CountryIdentityDocId} with supplied body fields.";
            s.Params["CountryIdentityDocId"] = "Association GUID (from route)";

            s.Response<bool>(200, "Returns true if update succeeded");
            s.Response(400, "Validation or business rule failure");
            s.Response(404, "Association not found");
            s.Response(409, "Conflict with an existing association");
            s.Response(500, "Unexpected server error");
        });
        Options(o => o.WithTags(EndpointGroups.CountryIdentityDocs));
    }

    public override async Task HandleAsync(UpdateCountryIdentityDocRequest req, CancellationToken ct)
    {
        var cmd = req.Adapt<UpdateCountryIdentityDocCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}