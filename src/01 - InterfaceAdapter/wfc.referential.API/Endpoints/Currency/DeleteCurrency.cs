using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Currencies.Commands.DeleteCurrency;
using wfc.referential.Application.Currencies.Dtos;

namespace wfc.referential.API.Endpoints.Currency;

public class DeleteCurrency(IMediator _mediator) : Endpoint<DeleteCurrencyRequest, bool>
{
    public override void Configure()
    {
        Delete("/api/currencies/{CurrencyId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Delete a Currency by GUID";
            s.Description = "Deletes the Currency identified by {CurrencyId}, as route param. " +
                           "If the currency is associated with countries, it will be marked as inactive instead of being deleted.";
            s.Response<bool>(200, "True if deletion or inactivation succeeded");
            s.Response(400, "If the 'CurrencyId' is invalid");
            s.Response(404, "If the currency does not exist");
        });
        Options(o => o.WithTags(EndpointGroups.Currencies));
    }

    public override async Task HandleAsync(DeleteCurrencyRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<DeleteCurrencyCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}