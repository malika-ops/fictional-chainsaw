using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.CountryIdentityDocs.Commands.CreateCountryIdentityDoc;
using wfc.referential.Application.CountryIdentityDocs.Dtos;

namespace wfc.referential.API.Endpoints.CountryIdentityDoc;

public class CreateCountryIdentityDocEndpoint(IMediator _mediator) : Endpoint<CreateCountryIdentityDocRequest, Guid>
{
    public override void Configure()
    {
        Post("/api/countryidentitydocs");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new Country-Identity Document association";
            s.Description = "Creates an association between a Country and an Identity Document.";
            s.Response<Guid>(200, "Identifier of the newly created association");
            s.Response(400, "Validation failed");
            s.Response(409, "Conflict with an existing association");
            s.Response(500, "Unexpected server error");
        });
        Options(o => o.WithTags(EndpointGroups.CountryIdentityDocs));
    }

    public override async Task HandleAsync(CreateCountryIdentityDocRequest req, CancellationToken ct)
    {
        var command = req.Adapt<CreateCountryIdentityDocCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}