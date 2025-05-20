using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.CountryIdentityDocs.Commands.UpdateCountryIdentityDoc;
using wfc.referential.Application.CountryIdentityDocs.Dtos;

namespace wfc.referential.API.Endpoints.CountryIdentityDoc;

public class UpdateCountryIdentityDoc(IMediator _mediator) : Endpoint<UpdateCountryIdentityDocRequest, Guid>
{
    public override void Configure()
    {
        Put("/api/countryidentitydocs/{CountryIdentityDocId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update an existing Country-Identity Document association";
            s.Description = "Updates the association identified by CountryIdentityDocId with new country, identity document, and enabled status.";
            s.Params["CountryIdentityDocId"] = "Association ID (GUID) from route";

            s.Response<Guid>(200, "Returns the ID of the updated association upon success");
            s.Response(400, "Returned if validation fails (missing fields, invalid ID, etc.)");
            s.Response(404, "Returned if the association does not exist");
            s.Response(500, "Server error if something unexpected occurs");
        });

        Options(o => o.WithTags(EndpointGroups.CountryIdentityDocs));
    }

    public override async Task HandleAsync(UpdateCountryIdentityDocRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<UpdateCountryIdentityDocCommand>();
        var associationId = await _mediator.Send(command, ct);
        await SendAsync(associationId, cancellation: ct);
    }
}