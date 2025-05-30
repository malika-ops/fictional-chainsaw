using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.CountryIdentityDocs.Commands.PatchCountryIdentityDoc;
using wfc.referential.Application.CountryIdentityDocs.Dtos;

namespace wfc.referential.API.Endpoints.CountryIdentityDoc;

public class PatchCountryIdentityDocEndpoint(IMediator _mediator)
    : Endpoint<PatchCountryIdentityDocRequest, bool>
{
    public override void Configure()
    {
        Patch("/api/countryidentitydocs/{CountryIdentityDocId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Partially update a Country-Identity Document association";
            s.Description =
                "Updates only the supplied fields for the association identified by {CountryIdentityDocId}.";
            s.Params["CountryIdentityDocId"] = "Association GUID from route";

            s.Response<bool>(200, "Returns true if update succeeded");
            s.Response(400, "Validation / business rule failure");
            s.Response(404, "Association not found");
            s.Response(409, "Conflict with an existing association");
        });
        Options(o => o.WithTags(EndpointGroups.CountryIdentityDocs));
    }

    public override async Task HandleAsync(PatchCountryIdentityDocRequest req, CancellationToken ct)
    {
        var cmd = req.Adapt<PatchCountryIdentityDocCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}