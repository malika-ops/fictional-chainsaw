using BuildingBlocks.Core.Pagination;
using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.IdentityDocuments.Commands.CreateIdentityDocument;
using wfc.referential.Application.IdentityDocuments.Dtos;

namespace wfc.referential.API.Endpoints.IdentityDocument;

public class CreateIdentityDocument(IMediator _mediator) : Endpoint<CreateIdentityDocumentRequest, Guid>
{
    public override void Configure()
    {
        Post("api/identitydocuments");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create IdentityDocument";
            s.Response<PagedResult<CreateIdentityDocumentResponse>>(201, "IdentityDocument Created Successfully");
        });
        Options(o => o.WithTags(EndpointGroups.IdentityDocuments));
    }

    public override async Task HandleAsync(CreateIdentityDocumentRequest req, CancellationToken ct)
    {
        var IdentityDocumentCommand = req.Adapt<CreateIdentityDocumentCommand>();
        var result = await _mediator.Send(IdentityDocumentCommand, ct);
        await SendAsync(result.Value, 201, ct);
    }
}