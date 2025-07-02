using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.Affiliates.Commands.CreateAffiliate;
using wfc.referential.Application.Affiliates.Commands.DeleteAffiliate;
using wfc.referential.Application.Affiliates.Commands.PatchAffiliate;
using wfc.referential.Application.Affiliates.Commands.UpdateAffiliate;
using wfc.referential.Application.Affiliates.Dtos;
using wfc.referential.Application.Affiliates.Queries.GetAffiliateById;
using wfc.referential.Application.Affiliates.Queries.GetFiltredAffiliates;

namespace wfc.referential.API.Endpoints;

public static class AffiliateEndpoints
{
    public static void MapAffiliateEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/affiliates")
            .WithTags("Affiliates");

        group.MapPost("/", CreateAffiliate)
            .WithName("CreateAffiliate")
            .WithSummary("Create a new Affiliate")
            .WithDescription("Creates an affiliate with code, name, country, and other relevant information.")
            .Produces<Guid>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapGet("/", GetFiltredAffiliates)
            .WithName("GetFiltredAffiliates")
            .WithSummary("Get paginated list of affiliates")
            .WithDescription("Returns a paginated list of affiliates. Filters supported: code, name, opening date, cancellation date, country ID, and enabled status.")
            .Produces<PagedResult<GetAffiliatesResponse>>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapGet("/{affiliateId:guid}", GetAffiliateById)
            .WithName("GetAffiliateById")
            .WithSummary("Get an Affiliate by GUID")
            .WithDescription("Retrieves the Affiliate identified by affiliateId.")
            .Produces<GetAffiliatesResponse>(200)
            .Produces(404)
            .ProducesValidationProblem(400);

        group.MapPut("/{affiliateId:guid}", UpdateAffiliate)
            .WithName("UpdateAffiliate")
            .WithSummary("Update an existing Affiliate")
            .WithDescription("Updates the affiliate identified by affiliateId with supplied body fields.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapPatch("/{affiliateId:guid}", PatchAffiliate)
            .WithName("PatchAffiliate")
            .WithSummary("PartiFiltredy update an Affiliate")
            .WithDescription("Updates only the supplied fields for the affiliate identified by affiliateId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409);

        group.MapDelete("/{affiliateId:guid}", DeleteAffiliate)
            .WithName("DeleteAffiliate")
            .WithSummary("Delete an affiliate by GUID")
            .WithDescription("Soft-deletes the affiliate identified by affiliateId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409);
    }

    internal static async Task<IResult> CreateAffiliate(
        CreateAffiliateRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreateAffiliateCommand>();
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> GetFiltredAffiliates(
        [AsParameters] GetFiltredAffiliatesRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFiltredAffiliatesQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> GetAffiliateById(
        Guid affiliateId,
        IMediator mediator)
    {
        var query = new GetAffiliateByIdQuery { AffiliateId = affiliateId };
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdateAffiliate(
        Guid affiliateId,
        UpdateAffiliateRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdateAffiliateCommand>();
        command = command with { AffiliateId = affiliateId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchAffiliate(
        Guid affiliateId,
        PatchAffiliateRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchAffiliateCommand>();
        command = command with { AffiliateId = affiliateId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeleteAffiliate(
        Guid affiliateId,
        IMediator mediator)
    {
        var command = new DeleteAffiliateCommand { AffiliateId = affiliateId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}