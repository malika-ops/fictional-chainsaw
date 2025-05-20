using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Products.Commands.DeleteProduct;
using wfc.referential.Application.Products.Dtos;

namespace wfc.referential.API.Endpoints.Product;

public class DeleteProduct(IMediator _mediator) : Endpoint<DeleteProductRequest, bool>
{
    public override void Configure()
    {
        Delete("api/products/{ProductId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Delete a Product";
        });
        Options(o => o.WithTags(EndpointGroups.Products));
    }

    public override async Task HandleAsync(DeleteProductRequest req, CancellationToken ct)
    {
        var command = req.Adapt<DeleteProductCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}