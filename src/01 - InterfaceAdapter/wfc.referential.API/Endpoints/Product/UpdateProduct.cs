using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.Products.Commands.UpdateProduct;
using wfc.referential.Application.Products.Dtos;

namespace wfc.referential.API.Endpoints.Product;

public class PutProduct(IMediator _mediator) : Endpoint<UpdateProductRequest, Guid>
{
    public override void Configure()
    {
        Put("api/products/{ProductId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Fully update a Product's properties";
            s.Description = "Updates all fields (code, name, status, countryId) of the specified Product ID.";
            s.Params["ProductId"] = "Product ID (GUID) from route";

            s.Response<Guid>(200, "The ID of the updated Product");
            s.Response(400, "If validation fails or the ID is invalid");
            s.Response(404, "Product not found");
        });

        Options(o => o.WithTags(EndpointGroups.Products));
    }

    public override async Task HandleAsync(UpdateProductRequest req, CancellationToken ct)
    {
        var command = req.Adapt<UpdateProductCommand>();

        var updatedId = await _mediator.Send(command, ct);

        await SendAsync(updatedId.Value, cancellation: ct);
    }
}
