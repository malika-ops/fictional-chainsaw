using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.CountryServices.Commands.CreateCountryService;
using wfc.referential.Application.CountryServices.Dtos;

namespace wfc.referential.API.Endpoints.CountryServices;

public class CreateCountryServices(IMediator _mediator) : Endpoint<CreateCountryServiceRequest, Guid>
{
    public override void Configure()
    {
        Post("/api/countryservices");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new Country-Service association";
            s.Description = "Creates an association between a Country and an Service.";
            s.Response<Guid>(200, "Identifier of the newly created association");
            s.Response(400, "Validation failed");
            s.Response(409, "Conflict with an existing association");
            s.Response(500, "Unexpected server error");
        });
        Options(o => o.WithTags(EndpointGroups.CountryServices));
    }

    public override async Task HandleAsync(CreateCountryServiceRequest req, CancellationToken ct)
    {
        var command = req.Adapt<CreateCountryServiceCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}