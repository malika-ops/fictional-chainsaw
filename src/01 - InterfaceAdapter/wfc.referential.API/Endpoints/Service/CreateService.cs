using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Services.Commands.CreateService;
using wfc.referential.Application.Services.Dtos;

namespace wfc.referential.API.Endpoints.Service;

public class CreateService(IMediator _mediator) : Endpoint<CreateServiceRequest, Guid>
{
    public override void Configure()
    {
        Post("api/services");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create Service";
            s.Response<PagedResult<CreateServiceResponse>>(201, "Product Created Successfully");
        });
        Options(o => o.WithTags(EndpointGroups.Services));
    }

    public override async Task HandleAsync(CreateServiceRequest req, CancellationToken ct)
    {
        var command = req.Adapt<CreateServiceCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, 201, ct);
    }
}