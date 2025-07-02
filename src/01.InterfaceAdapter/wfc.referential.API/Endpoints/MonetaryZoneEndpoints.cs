using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.MonetaryZones.Commands.CreateMonetaryZone;
using wfc.referential.Application.MonetaryZones.Commands.DeleteMonetaryZone;
using wfc.referential.Application.MonetaryZones.Commands.PatchMonetaryZone;
using wfc.referential.Application.MonetaryZones.Commands.UpdateMonetaryZone;
using wfc.referential.Application.MonetaryZones.Dtos;
using wfc.referential.Application.MonetaryZones.Queries.GetMonetaryZoneById;
using wfc.referential.Application.MonetaryZones.Queries.GetFiltredMonetaryZones;

namespace wfc.referential.API.Features;

public static class MonetaryZoneEndpoints
{
    public static void MapMonetaryZoneEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/monetaryZones")
            .WithTags("MonetaryZones");

        group.MapPost("/", CreateMonetaryZone)
            .WithName("CreateMonetaryZone")
            .WithSummary("Create a new MonetaryZone")
            .WithDescription("Creates a monetary zone with code, name, and description. Code must be unique and name/description are required.")
            .Produces<Guid>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapGet("/", GetFiltredMonetaryZones)
            .WithName("GetFiltredMonetaryZones")
            .WithSummary("Get paginated list of MonetaryZones")
            .WithDescription("Returns a paginated list of MonetaryZones. Supports optional filtering by code, name, and description.")
            .Produces<PagedResult<MonetaryZoneResponse>>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapGet("/{monetaryZoneId:guid}", GetMonetaryZoneById)
            .WithName("GetMonetaryZoneById")
            .WithSummary("Get a MonetaryZone by GUID")
            .WithDescription("Retrieves the MonetaryZone identified by monetaryZoneId.")
            .Produces<MonetaryZoneResponse>(200)
            .Produces(404)
            .ProducesValidationProblem(400);

        group.MapPut("/{monetaryZoneId:guid}", UpdateMonetaryZone)
            .WithName("UpdateMonetaryZone")
            .WithSummary("Update an existing MonetaryZone")
            .WithDescription("Updates the zone identified by monetaryZoneId with new code, name, description.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapPatch("/{monetaryZoneId:guid}", PatchMonetaryZone)
            .WithName("PatchMonetaryZone")
            .WithSummary("Partially update a MonetaryZone's properties")
            .WithDescription("Updates only the provided fields (code, name, or description) of the specified zone ID.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409);

        group.MapDelete("/{monetaryZoneId:guid}", DeleteMonetaryZone)
            .WithName("DeleteMonetaryZone")
            .WithSummary("Delete a MonetaryZone by GUID")
            .WithDescription("Deletes the MonetaryZone identified by monetaryZoneId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404);
    }

    internal static async Task<IResult> CreateMonetaryZone(
        CreateMonetaryZoneRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreateMonetaryZoneCommand>();
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> GetFiltredMonetaryZones(
        [AsParameters] GetFiltredMonetaryZonesRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFiltredMonetaryZonesQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> GetMonetaryZoneById(
        Guid monetaryZoneId,
        IMediator mediator)
    {
        var query = new GetMonetaryZoneByIdQuery { MonetaryZoneId = monetaryZoneId };
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdateMonetaryZone(
        Guid monetaryZoneId,
        UpdateMonetaryZoneRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdateMonetaryZoneCommand>();
        command = command with { MonetaryZoneId = monetaryZoneId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchMonetaryZone(
        Guid monetaryZoneId,
        PatchMonetaryZoneRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchMonetaryZoneCommand>();
        command = command with { MonetaryZoneId = monetaryZoneId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeleteMonetaryZone(
        Guid monetaryZoneId,
        IMediator mediator)
    {
        var command = new DeleteMonetaryZoneCommand { MonetaryZoneId = monetaryZoneId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}