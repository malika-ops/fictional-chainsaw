using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.Pricings.Commands.CreatePricing;
using wfc.referential.Application.Pricings.Commands.DeletePricing;
using wfc.referential.Application.Pricings.Commands.PatchPricing;
using wfc.referential.Application.Pricings.Commands.UpdatePricing;
using wfc.referential.Application.Pricings.Dtos;
using wfc.referential.Application.Pricings.Queries.GetFiltredPricings;
using wfc.referential.Application.Pricings.Queries.GetPricingById;

namespace wfc.referential.API.Endpoints;

public static class PricingEndpoints
{
    public static void MapPricingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/pricings")
            .WithTags("Pricings");

        group.MapPost("/", CreatePricing)
            .WithName("CreatePricing")
            .WithSummary("Create a new Pricing line")
            .WithDescription("Adds a tariff line (pricing) for a given service & corridor. Either FixedAmount or Rate must be supplied.")
            .Produces<Guid>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapGet("/", GetFiltredPricings)
            .WithName("GetFiltredPricings")
            .WithSummary("Get paginated list of Pricing lines")
            .WithDescription("Returns a paginated list of pricings. Supports optional filtering by Code, Channel, CorridorId, ServiceId, AffiliateId and IsEnabled.")
            .Produces<PagedResult<PricingResponse>>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapGet("/{pricingId:guid}", GetPricingById)
            .WithName("GetPricingById")
            .WithSummary("Get a Pricing by GUID")
            .WithDescription("Retrieves the Pricing identified by pricingId.")
            .Produces<PricingResponse>(200)
            .Produces(404)
            .ProducesValidationProblem(400);

        group.MapPut("/{pricingId:guid}", UpdatePricing)
            .WithName("UpdatePricing")
            .WithSummary("Update an existing Pricing line")
            .WithDescription("Replaces every mutable attribute of an existing pricing (Code, Channel, min/max amounts, FixedAmount, Rate, etc.).")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(409);

        group.MapPatch("/{pricingId:guid}", PatchPricing)
            .WithName("PatchPricing")
            .WithSummary("Partially update an existing Pricing line")
            .WithDescription("Updates only the supplied fields (Code, Channel, min/max amounts, FixedAmount, Rate, CorridorId, ServiceId, AffiliateId, IsEnabled).")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(409);

        group.MapDelete("/{pricingId:guid}", DeletePricing)
            .WithName("DeletePricing")
            .WithSummary("Soft-delete a Pricing row by GUID")
            .WithDescription("Sets IsEnabled = false on the specified pricing and raises PricingDisabledEvent.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404);
    }

    internal static async Task<IResult> GetPricingById(
        Guid pricingId,
        IMediator mediator)
    {
        var query = new GetPricingByIdQuery { PricingId = pricingId };
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> CreatePricing(
        CreatePricingRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreatePricingCommand>();
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> GetFiltredPricings(
        [AsParameters] GetFiltredPricingsRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFiltredPricingsQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdatePricing(
        Guid pricingId,
        UpdatePricingRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdatePricingCommand>();
        command = command with { PricingId = pricingId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchPricing(
        Guid pricingId,
        PatchPricingRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchPricingCommand>();
        command = command with { PricingId = pricingId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeletePricing(
        Guid pricingId,
        IMediator mediator)
    {
        var command = new DeletePricingCommand { PricingId = pricingId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}