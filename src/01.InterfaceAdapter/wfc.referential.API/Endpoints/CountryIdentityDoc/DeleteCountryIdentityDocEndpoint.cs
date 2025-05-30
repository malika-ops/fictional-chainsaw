using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.CountryIdentityDocs.Commands.DeleteCountryIdentityDoc;
using wfc.referential.Application.CountryIdentityDocs.Dtos;

namespace wfc.referential.API.Endpoints.CountryIdentityDoc;

public class DeleteCountryIdentityDocEndpoint(IMediator _mediator)
    : Endpoint<DeleteCountryIdentityDocRequest, bool>
{
    public override void Configure()
    {
        Delete("/api/countryidentitydocs/{CountryIdentityDocId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Delete a country-identity document association by GUID";
            s.Description = "Soft-deletes the association identified by {CountryIdentityDocId}.";
            s.Params["CountryIdentityDocId"] = "GUID of the association to delete";
            s.Response<bool>(200, "True if deletion succeeded");
            s.Response(400, "Validation / business rule failure");
            s.Response(404, "Association not found");
        });
        Options(o => o.WithTags(EndpointGroups.CountryIdentityDocs));
    }

    public override async Task HandleAsync(DeleteCountryIdentityDocRequest req, CancellationToken ct)
    {
        var command = req.Adapt<DeleteCountryIdentityDocCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}