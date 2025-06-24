using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.IdentityDocuments.Commands.CreateIdentityDocument;
using wfc.referential.Application.IdentityDocuments.Commands.DeleteIdentityDocument;
using wfc.referential.Application.IdentityDocuments.Commands.PatchIdentityDocument;
using wfc.referential.Application.IdentityDocuments.Commands.UpdateIdentityDocument;
using wfc.referential.Application.IdentityDocuments.Dtos;
using wfc.referential.Application.IdentityDocuments.Queries.GetFiltredIdentityDocuments;

namespace wfc.referential.API.Features;

public static class IdentityDocumentEndpoints
{
    public static void MapIdentityDocumentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/identitydocuments")
            .WithTags("IdentityDocuments");

        group.MapPost("/", CreateIdentityDocument)
            .WithName("CreateIdentityDocument")
            .WithSummary("Create a new Identity Document")
            .WithDescription("Creates an identity document with the provided code, name, and description.")
            .Produces<Guid>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapGet("/", GetFiltredIdentityDocuments)
            .WithName("GetFiltredIdentityDocuments")
            .WithSummary("Get paginated list of identity documents")
            .WithDescription("Returns a paginated list of identity documents. Filters supported: code, name, status.")
            .Produces<PagedResult<GetIdentityDocumentsResponse>>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapPut("/{identityDocumentId:guid}", UpdateIdentityDocument)
            .WithName("UpdateIdentityDocument")
            .WithSummary("Update an existing Identity Document")
            .WithDescription("Updates the identity document identified by identityDocumentId with supplied body fields.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapPatch("/{identityDocumentId:guid}", PatchIdentityDocument)
            .WithName("PatchIdentityDocument")
            .WithSummary("Partially update an Identity Document")
            .WithDescription("Updates only the supplied fields for the identity document identified by identityDocumentId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409);

        group.MapDelete("/{identityDocumentId:guid}", DeleteIdentityDocument)
            .WithName("DeleteIdentityDocument")
            .WithSummary("Delete an identity document by GUID")
            .WithDescription("Soft-deletes the identity document identified by identityDocumentId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404);
    }

    internal static async Task<IResult> CreateIdentityDocument(
        CreateIdentityDocumentRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreateIdentityDocumentCommand>();
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> GetFiltredIdentityDocuments(
        [AsParameters] GetFiltredIdentityDocumentsRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFiltredIdentityDocumentsQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdateIdentityDocument(
        Guid identityDocumentId,
        UpdateIdentityDocumentRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdateIdentityDocumentCommand>();
        command = command with { IdentityDocumentId = identityDocumentId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchIdentityDocument(
        Guid identityDocumentId,
        PatchIdentityDocumentRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchIdentityDocumentCommand>();
        command = command with { IdentityDocumentId = identityDocumentId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeleteIdentityDocument(
        Guid identityDocumentId,
        IMediator mediator)
    {
        var command = new DeleteIdentityDocumentCommand { IdentityDocumentId = identityDocumentId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}