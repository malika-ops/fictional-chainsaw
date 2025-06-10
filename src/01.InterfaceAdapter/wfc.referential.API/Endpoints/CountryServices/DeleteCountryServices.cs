using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.CountryServices.Commands.DeleteCountryService;
using wfc.referential.Application.CountryServices.Dtos;
namespace wfc.referential.API.Endpoints.CountryServices;

public class DeleteCountryServices(IMediator _mediator)
    : Endpoint<DeleteCountryServiceRequest, bool>
{
    public override void Configure()
    {
        Delete("/api/countryservices/{CountryServiceId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Delete a country-service document association by GUID";
            s.Description = "Soft-deletes the association identified by {CountryServiceId}.";
            s.Params["CountryServiceId"] = "GUID of the association to delete";
            s.Response<bool>(200, "True if deletion succeeded");
            s.Response(400, "Validation / business rule failure");
            s.Response(404, "Association not found");
        });
        Options(o => o.WithTags(EndpointGroups.CountryServices));
    }

    public override async Task HandleAsync(DeleteCountryServiceRequest req, CancellationToken ct)
    {
        var command = req.Adapt<DeleteCountryServiceCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}