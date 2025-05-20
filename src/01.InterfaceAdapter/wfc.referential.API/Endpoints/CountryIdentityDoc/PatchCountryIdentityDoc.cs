using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.CountryIdentityDocs.Commands.PatchCountryIdentityDoc;
using wfc.referential.Application.CountryIdentityDocs.Dtos;

namespace wfc.referential.API.Endpoints.CountryIdentityDoc;

public class PatchCountryIdentityDoc(IMediator _mediator) : Endpoint<PatchCountryIdentityDocRequest, Guid>
{
    public override void Configure()
    {
        Patch("/api/countryidentitydocs/{CountryIdentityDocId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Partially update a Country-Identity Document association's properties";
            s.Description = "Updates only the provided fields of the specified association ID.";
            s.Params["CountryIdentityDocId"] = "Association ID (GUID) from route";

            s.Response<Guid>(200, "The ID of the updated association");
            s.Response(400, "If validation fails or the ID is invalid");
            s.Response(404, "Association not found");
        });

        Options(o => o.WithTags(EndpointGroups.CountryIdentityDocs));
    }

    public override async Task HandleAsync(PatchCountryIdentityDocRequest req, CancellationToken ct)
    {
        var command = req.Adapt<PatchCountryIdentityDocCommand>();
        var updatedId = await _mediator.Send(command, ct);
        await SendAsync(updatedId, cancellation: ct);
    }
}