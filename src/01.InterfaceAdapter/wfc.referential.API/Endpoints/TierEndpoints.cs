using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.Tiers.Commands.CreateTier;
using wfc.referential.Application.Tiers.Commands.DeleteTier;
using wfc.referential.Application.Tiers.Commands.PatchTier;
using wfc.referential.Application.Tiers.Commands.UpdateTier;
using wfc.referential.Application.Tiers.Dtos;
using wfc.referential.Application.Tiers.Queries.GetFiltredTiers;

namespace wfc.referential.API.Endpoints;

public static class TierEndpoints
{
    public static void MapTierEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/tiers")
            .WithTags("Tiers");

        group.MapPost("/", CreateTier)
            .WithName("CreateTier")
            .WithSummary("Create a new Tier")
            .WithDescription("Creates a Tier with Name and Description. Name must be unique.")
            .Produces<Guid>(201)
            .ProducesValidationProblem(400)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapGet("/", GetFiltredTiers)
            .WithName("GetFiltredTiers")
            .WithSummary("Get paginated list of Tiers")
            .WithDescription("Returns a paginated list of tiers with optional filters on name, description and enabled flag.")
            .Produces<PagedResult<TierResponse>>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapPut("/{tierId:guid}", UpdateTier)
            .WithName("UpdateTier")
            .WithSummary("Update an existing Tier")
            .WithDescription("Replaces Name, Description and IsEnabled of the specified Tier.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(409);

        group.MapPatch("/{tierId:guid}", PatchTier)
            .WithName("PatchTier")
            .WithSummary("Partially update a Tier")
            .WithDescription("Updates only the supplied fields (Name, Description, IsEnabled).")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(409);

        group.MapDelete("/{tierId:guid}", DeleteTier)
            .WithName("DeleteTier")
            .WithSummary("Delete a Tier by GUID")
            .WithDescription("Soft-deletes (disables) the Tier identified by tierId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400);
    }

    internal static async Task<IResult> CreateTier(
        CreateTierRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreateTierCommand>();
        var result = await mediator.Send(command);
        return Results.Created($"/api/tiers/{result.Value}", result.Value);
    }

    internal static async Task<IResult> GetFiltredTiers(
        [AsParameters] GetFiltredTiersRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFiltredTiersQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdateTier(
        Guid tierId,
        UpdateTierRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdateTierCommand>();
        command = command with { TierId = tierId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchTier(
        Guid tierId,
        PatchTierRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchTierCommand>();
        command = command with { TierId = tierId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeleteTier(
        Guid tierId,
        IMediator mediator)
    {
        var command = new DeleteTierCommand { TierId = tierId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}