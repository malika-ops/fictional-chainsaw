using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.TaxRuleDetails.Commands.CreateTaxRuleDetail;
using wfc.referential.Application.TaxRuleDetails.Commands.DeleteTaxRuleDetail;
using wfc.referential.Application.TaxRuleDetails.Commands.PatchTaxRuleDetail;
using wfc.referential.Application.TaxRuleDetails.Commands.UpdateTaxRuleDetail;
using wfc.referential.Application.TaxRuleDetails.Dtos;
using wfc.referential.Application.TaxRuleDetails.Queries.GetFiltredTaxeRuleDetails;

namespace wfc.referential.API.Endpoints;

public static class TaxRuleDetailEndpoints
{
    public static void MapTaxRuleDetailEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/tax-rule-details")
            .WithTags("TaxRuleDetails");

        group.MapPost("/", CreateTaxRuleDetail)
            .WithName("CreateTaxRuleDetail")
            .WithSummary("Create tax Rule Detail")
            .WithDescription("Creates a new tax rule detail with the provided tax rule configuration and details.")
            .Produces<Guid>(201)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapGet("/", GetFiltredTaxRuleDetails)
            .WithName("GetFiltredTaxRuleDetails")
            .WithSummary("Get paginated list of tax rule details")
            .WithDescription("Returns a paginated list of tax rule details. Supports optional filtering by rule ID, tax ID, status, and other rule configuration parameters.")
            .Produces<PagedResult<GetFiltredTaxRuleDetailsResponse>>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapPut("/{taxRuleDetailId:guid}", UpdateTaxRuleDetail)
            .WithName("UpdateTaxRuleDetail")
            .WithSummary("Fully update a Tax Rule Detail's properties")
            .WithDescription("Updates all fields of the specified Tax Rule Detail ID.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404);

        group.MapPatch("/{taxRuleDetailId:guid}", PatchTaxRuleDetail)
            .WithName("PatchTaxRuleDetail")
            .WithSummary("Partially update a Tax Rule Detail's properties")
            .WithDescription("Updates only the provided fields (rule configuration, tax ID, status, or other rule parameters) of the specified Tax Rule Detail ID.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404);

        group.MapDelete("/{taxRuleDetailId:guid}", DeleteTaxRuleDetail)
            .WithName("DeleteTaxRuleDetail")
            .WithSummary("Delete a tax rule detail")
            .WithDescription("Soft-deletes the tax rule detail identified by taxRuleDetailId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404);
    }

    internal static async Task<IResult> CreateTaxRuleDetail(
        CreateTaxRuleDetailRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreateTaxRuleDetailCommand>();
        var result = await mediator.Send(command);
        return Results.Created($"/api/tax-rule-details/{result.Value}", result.Value);
    }

    internal static async Task<IResult> GetFiltredTaxRuleDetails(
        [AsParameters] GetFiltredTaxRuleDetailsRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFiltredTaxRuleDetailsQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdateTaxRuleDetail(
        Guid taxRuleDetailId,
        UpdateTaxRuleDetailRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdateTaxRuleDetailCommand>();
        command = command with { TaxRuleDetailsId = taxRuleDetailId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchTaxRuleDetail(
        Guid taxRuleDetailId,
        PatchTaxRuleDetailRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchTaxRuleDetailCommand>();
        command = command with { TaxRuleDetailsId = taxRuleDetailId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeleteTaxRuleDetail(
        Guid taxRuleDetailId,
        IMediator mediator)
    {
        var command = new DeleteTaxRuleDetailCommand { TaxRuleDetailId = taxRuleDetailId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}