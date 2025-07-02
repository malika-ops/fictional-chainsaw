using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.TypeDefinitions.Commands.CreateTypeDefinition;
using wfc.referential.Application.TypeDefinitions.Commands.DeleteTypeDefinition;
using wfc.referential.Application.TypeDefinitions.Commands.PatchTypeDefinition;
using wfc.referential.Application.TypeDefinitions.Commands.UpdateTypeDefinition;
using wfc.referential.Application.TypeDefinitions.Dtos;
using wfc.referential.Application.TypeDefinitions.Queries.GetTypeDefinitionById;
using wfc.referential.Application.TypeDefinitions.Queries.GetFiltredTypeDefinitions;

namespace wfc.referential.API.Endpoints;

public static class TypeDefinitionEndpoints
{
    public static void MapTypeDefinitionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/type-definitions")
            .WithTags("TypeDefinitions");

        group.MapPost("/", CreateTypeDefinition)
            .WithName("CreateTypeDefinition")
            .WithSummary("Create a new TypeDefinition")
            .WithDescription("Creates a TypeDefinition with the provided libelle and description.")
            .Produces<Guid>(201)
            .ProducesValidationProblem(400)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapGet("/", GetFiltredTypeDefinitions)
            .WithName("GetFiltredTypeDefinitions")
            .WithSummary("Get paginated list of TypeDefinitions")
            .WithDescription("Returns a paginated list of TypeDefinitions. Supports optional filtering by Libelle and description.")
            .Produces<PagedResult<GetTypeDefinitionsResponse>>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapGet("/{typeDefinitionId:guid}", GetTypeDefinitionById)
            .WithName("GetTypeDefinitionById")
            .WithSummary("Get a TypeDefinition by GUID")
            .WithDescription("Retrieves the TypeDefinition identified by typeDefinitionId.")
            .Produces<GetTypeDefinitionsResponse>(200)
            .Produces(404)
            .ProducesValidationProblem(400);

        group.MapPut("/{typeDefinitionId:guid}", UpdateTypeDefinition)
            .WithName("UpdateTypeDefinition")
            .WithSummary("Update an existing TypeDefinition")
            .WithDescription("Updates the TypeDefinition identified by typeDefinitionId with supplied body fields.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapPatch("/{typeDefinitionId:guid}", PatchTypeDefinition)
            .WithName("PatchTypeDefinition")
            .WithSummary("Partially update a TypeDefinition")
            .WithDescription("Updates only the supplied fields for the TypeDefinition identified by typeDefinitionId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(409);

        group.MapDelete("/{typeDefinitionId:guid}", DeleteTypeDefinition)
            .WithName("DeleteTypeDefinition")
            .WithSummary("Delete a TypeDefinition by GUID")
            .WithDescription("Soft-deletes the TypeDefinition identified by typeDefinitionId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(409);
    }

    internal static async Task<IResult> GetTypeDefinitionById(
        Guid typeDefinitionId,
        IMediator mediator)
    {
        var query = new GetTypeDefinitionByIdQuery { TypeDefinitionId = typeDefinitionId };
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> CreateTypeDefinition(
        CreateTypeDefinitionRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreateTypeDefinitionCommand>();
        var result = await mediator.Send(command);
        return Results.Created($"/api/type-definitions/{result.Value}", result.Value);
    }

    internal static async Task<IResult> GetFiltredTypeDefinitions(
        [AsParameters] GetFiltredTypeDefinitionsRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFiltredTypeDefinitionsQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdateTypeDefinition(
        Guid typeDefinitionId,
        UpdateTypeDefinitionRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdateTypeDefinitionCommand>();
        command = command with { TypeDefinitionId = typeDefinitionId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchTypeDefinition(
        Guid typeDefinitionId,
        PatchTypeDefinitionRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchTypeDefinitionCommand>();
        command = command with { TypeDefinitionId = typeDefinitionId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeleteTypeDefinition(
        Guid typeDefinitionId,
        IMediator mediator)
    {
        var command = new DeleteTypeDefinitionCommand(typeDefinitionId);
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}