using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.Regions.Commands.CreateRegion;
using wfc.referential.Application.Regions.Commands.DeleteRegion;
using wfc.referential.Application.Regions.Commands.PatchRegion;
using wfc.referential.Application.Regions.Commands.UpdateRegion;
using wfc.referential.Application.Regions.Dtos;
using wfc.referential.Application.RegionManagement.Dtos;
using wfc.referential.Application.RegionManagement.Queries.GetFiltredRegions;

namespace wfc.referential.API.Endpoints;

public static class RegionEndpoints
{
    public static void MapRegionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/regions")
            .WithTags("Regions");

        group.MapPost("/", CreateRegion)
            .WithName("CreateRegion")
            .WithSummary("Create region")
            .WithDescription("Creates a new region with the provided code, name, status, and country information.")
            .Produces<Guid>(201)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapGet("/", GetFiltredRegions)
            .WithName("GetFiltredRegions")
            .WithSummary("Get paginated list of regions")
            .WithDescription("Returns a paginated list of regions. Supports optional filtering by code, name, status, and countryId.")
            .Produces<PagedResult<GetFiltredRegionsResponse>>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapPut("/{regionId:guid}", UpdateRegion)
            .WithName("UpdateRegion")
            .WithSummary("Fully update a Region's properties")
            .WithDescription("Updates all fields (code, name, status, countryId) of the specified Region ID.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404);

        group.MapPatch("/{regionId:guid}", PatchRegion)
            .WithName("PatchRegion")
            .WithSummary("Partially update a Region's properties")
            .WithDescription("Updates only the provided fields (code, name, status or countryId) of the specified Region ID.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404);

        group.MapDelete("/{regionId:guid}", DeleteRegion)
            .WithName("DeleteRegion")
            .WithSummary("Delete a region")
            .WithDescription("Soft-deletes the region identified by regionId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404);
    }

    internal static async Task<IResult> CreateRegion(
        CreateRegionRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreateRegionCommand>();
        var result = await mediator.Send(command);
        return Results.Created($"/api/regions/{result.Value}", result.Value);
    }

    internal static async Task<IResult> GetFiltredRegions(
        [AsParameters] GetFiltredRegionsRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFiltredRegionsQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdateRegion(
        Guid regionId,
        UpdateRegionRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdateRegionCommand>();
        command = command with { RegionId = regionId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchRegion(
        Guid regionId,
        PatchRegionRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchRegionCommand>();
        command = command with { RegionId = regionId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeleteRegion(
        Guid regionId,
        IMediator mediator)
    {
        var command = new DeleteRegionCommand { RegionId = regionId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}