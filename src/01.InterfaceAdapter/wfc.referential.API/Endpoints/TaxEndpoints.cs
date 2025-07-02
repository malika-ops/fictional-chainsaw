using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.Taxes.Commands.CreateTax;
using wfc.referential.Application.Taxes.Commands.DeleteTax;
using wfc.referential.Application.Taxes.Commands.PatchTax;
using wfc.referential.Application.Taxes.Commands.UpdateTax;
using wfc.referential.Application.Taxes.Dtos;
using wfc.referential.Application.Taxes.Queries.GetTaxById;
using wfc.referential.Application.Taxes.Queries.GetFiltredTaxes;

namespace wfc.referential.API.Endpoints;

public static class TaxEndpoints
{
    public static void MapTaxEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/taxes")
            .WithTags("Taxes");

        group.MapPost("/", CreateTax)
            .WithName("CreateTax")
            .WithSummary("Create tax")
            .WithDescription("Creates a new tax with the provided code, name, rate, and country information.")
            .Produces<Guid>(201)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapGet("/", GetFiltredTaxes)
            .WithName("GetFiltredTaxes")
            .WithSummary("Get paginated list of taxes")
            .WithDescription("Returns a paginated list of taxes. Supports optional filtering by code, name, rate, status, and countryId.")
            .Produces<PagedResult<GetTaxesResponse>>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapGet("/{taxId:guid}", GetTaxById)
            .WithName("GetTaxById")
            .WithSummary("Get a Tax by GUID")
            .WithDescription("Retrieves the Tax identified by taxId.")
            .Produces<GetTaxesResponse>(200)
            .Produces(404)
            .ProducesValidationProblem(400);

        group.MapPut("/{taxId:guid}", UpdateTax)
            .WithName("UpdateTax")
            .WithSummary("Fully update a Tax's properties")
            .WithDescription("Updates all fields of the specified Tax ID.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404);

        group.MapPatch("/{taxId:guid}", PatchTax)
            .WithName("PatchTax")
            .WithSummary("Partially update a Tax's properties")
            .WithDescription("Updates only the provided fields (code, name, rate, status or countryId) of the specified Tax ID.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404);

        group.MapDelete("/{taxId:guid}", DeleteTax)
            .WithName("DeleteTax")
            .WithSummary("Delete a tax")
            .WithDescription("Soft-deletes the tax identified by taxId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(404);
    }

    internal static async Task<IResult> GetTaxById(
        Guid taxId,
        IMediator mediator)
    {
        var query = new GetTaxByIdQuery { TaxId = taxId };
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> CreateTax(
        CreateTaxRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreateTaxCommand>();
        var result = await mediator.Send(command);
        return Results.Created($"/api/taxes/{result.Value}", result.Value);
    }

    internal static async Task<IResult> GetFiltredTaxes(
        [AsParameters] GetFiltredTaxesRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFiltredTaxesQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdateTax(
        Guid taxId,
        UpdateTaxRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdateTaxCommand>();
        command = command with { TaxId = taxId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchTax(
        Guid taxId,
        PatchTaxRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchTaxCommand>();
        command = command with { TaxId = taxId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeleteTax(
        Guid taxId,
        IMediator mediator)
    {
        var command = new DeleteTaxCommand { TaxId = taxId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}