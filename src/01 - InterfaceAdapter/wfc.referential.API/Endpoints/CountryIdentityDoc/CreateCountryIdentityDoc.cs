using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.CountryIdentityDocs.Commands.CreateCountryIdentityDoc;
using wfc.referential.Application.CountryIdentityDocs.Dtos;

namespace wfc.referential.API.Endpoints.CountryIdentityDoc;

public class CreateCountryIdentityDoc(IMediator _mediator) : Endpoint<CreateCountryIdentityDocRequest, Guid>
{
    public override void Configure()
    {
        Post("/api/countryidentitydocs");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new Country-Identity Document association";
            s.Description = "Creates an association between a Country and an Identity Document. " +
                            "Both Country and Identity Document must exist.";

            s.Response<Guid>(200, "Returns the ID of the newly created association if successful");
            s.Response(400, "Returned if validation fails (missing fields or duplicate association)");
            s.Response(500, "Server error if an unexpected exception occurs");
        });

        Options(o => o.WithTags(EndpointGroups.CountryIdentityDocs));
    }

    public override async Task HandleAsync(CreateCountryIdentityDocRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<CreateCountryIdentityDocCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}