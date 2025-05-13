using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Currencies.Commands.CreateCurrency;
using wfc.referential.Application.Currencies.Dtos;

namespace wfc.referential.API.Endpoints.Currency;

public class CreateCurrency(IMediator _mediator) : Endpoint<CreateCurrencyRequest, Guid>
{
    public override void Configure()
    {
        Post("/api/currencies");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new Currency";
            s.Description = "Creates a currency with code, name, and status. " +
                           "Code must be unique and name is required.";
            s.Response<Guid>(201, "Returns the ID of the newly created Currency if successful");
            s.Response(400, "Returned if validation fails (missing fields or duplicate code)");
            s.Response(500, "Server error if an unexpected exception occurs");
        });
        Options(o => o.WithTags(EndpointGroups.Currencies));
    }

    public override async Task HandleAsync(CreateCurrencyRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<CreateCurrencyCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}