using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.AgencyTiers.Commands.CreateAgencyTier;
using wfc.referential.Application.AgencyTiers.Commands.DeleteAgencyTier;
using wfc.referential.Application.AgencyTiers.Commands.PatchAgencyTier;
using wfc.referential.Application.AgencyTiers.Commands.UpdateAgencyTier;
using wfc.referential.Application.AgencyTiers.Dtos;
using wfc.referential.Application.AgencyTiers.Queries.GetFiltredAgencyTiers;

namespace wfc.referential.API.Endpoints;

public static class AgencyTierEndpoints
{
    public static void MapAgencyTierEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/agencyTiers")
            .WithTags("AgencyTiers");

        group.MapPost("/", CreateAgencyTier)
            .WithName("CreateAgencyTier")
            .WithSummary("Create a new AgencyTier link")
            .WithDescription("Creates the association between an Agency and a Tier, with a unique Code and an optional Password. Code must be unique for the (Agency, Tier) pair.")
            .Produces<Guid>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapGet("/", GetFiltredAgencyTiers)
            .WithName("GetFiltredAgencyTiers")
            .WithSummary("Get paginated list of Agency-Tier mappings")
            .WithDescription("Returns a paginated list of AgencyTiers. Supports optional filtering by AgencyId, TierId, Code and IsEnabled.")
            .Produces<PagedResult<AgencyTierResponse>>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapPut("/{agencyTierId:guid}", UpdateAgencyTier)
            .WithName("UpdateAgencyTier")
            .WithSummary("Update an existing Agency-Tier link")
            .WithDescription("Replaces Code / Password / status (and optionally TierId, AgencyId) for the AgencyTier identified by the route param.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409);

        group.MapPatch("/{agencyTierId:guid}", PatchAgencyTier)
            .WithName("PatchAgencyTier")
            .WithSummary("Partially update an Agency-Tier link")
            .WithDescription("Only the supplied fields are updated (Code, Password, IsEnabled, AgencyId, TierId).")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409);

        group.MapDelete("/{agencyTierId:guid}", DeleteAgencyTier)
            .WithName("DeleteAgencyTier")
            .WithSummary("Delete an Agency-Tier mapping by GUID")
            .WithDescription("Soft-deletes the AgencyTier identified by agencyTierId by disabling it (IsEnabled = false).")
            .Produces<bool>(200)
            .ProducesValidationProblem(400);
    }

    internal static async Task<IResult> CreateAgencyTier(
        CreateAgencyTierRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreateAgencyTierCommand>();
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> GetFiltredAgencyTiers(
        [AsParameters] GetFiltredAgencyTiersRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFiltredAgencyTiersQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdateAgencyTier(
        Guid agencyTierId,
        UpdateAgencyTierRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdateAgencyTierCommand>();
        command = command with { AgencyTierId = agencyTierId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchAgencyTier(
        Guid agencyTierId,
        PatchAgencyTierRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchAgencyTierCommand>();
        command = command with { AgencyTierId = agencyTierId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeleteAgencyTier(
        Guid agencyTierId,
        IMediator mediator)
    {
        var command = new DeleteAgencyTierCommand { AgencyTierId = agencyTierId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}