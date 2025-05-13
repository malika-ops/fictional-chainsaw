using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Taxes.Commands.CreateTax;
using wfc.referential.Application.Taxes.Dtos;

namespace wfc.referential.API.Endpoints.Tax;

public class CreateTax(IMediator _mediator) : Endpoint<CreateTaxRequest, Guid>
{
    public override void Configure()
    {
        Post("api/taxes");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create tax";
            s.Response<PagedResult<CreateTaxResponse>>(200, "Successful Response");
        });
        Options(o => o.WithTags(EndpointGroups.Taxes));
    }

    public override async Task HandleAsync(CreateTaxRequest req, CancellationToken ct)
    {
        var taxCommand = req.Adapt<CreateTaxCommand>();
        var result = await _mediator.Send(taxCommand, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}