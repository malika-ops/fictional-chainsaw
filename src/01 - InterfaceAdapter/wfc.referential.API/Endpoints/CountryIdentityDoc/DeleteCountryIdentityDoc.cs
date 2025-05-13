using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.CountryIdentityDocs.Commands.DeleteCountryIdentityDoc;
using wfc.referential.Application.CountryIdentityDocs.Dtos;

namespace wfc.referential.API.Endpoints.CountryIdentityDoc;

public class DeleteCountryIdentityDoc(IMediator _mediator) : Endpoint<DeleteCountryIdentityDocRequest, bool>
{
    public override void Configure()
    {
        Delete("/api/countryidentitydocs/{CountryIdentityDocId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Delete a Country-Identity Document association by GUID";
            s.Description = "Disables the association identified by {CountryIdentityDocId}, as route param.";

            s.Response<bool>(200, "True if deletion succeeded");
            s.Response(400, "If deletion failed due to validation errors");
            s.Response(404, "If association was not found");
        });

        Options(o => o.WithTags(EndpointGroups.CountryIdentityDocs));
    }

    public override async Task HandleAsync(DeleteCountryIdentityDocRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<DeleteCountryIdentityDocCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result, cancellation: ct);
    }
}