using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.CountryIdentityDocs.Commands.CreateCountryIdentityDoc;
using wfc.referential.Application.CountryIdentityDocs.Commands.DeleteCountryIdentityDoc;
using wfc.referential.Application.CountryIdentityDocs.Commands.PatchCountryIdentityDoc;
using wfc.referential.Application.CountryIdentityDocs.Commands.UpdateCountryIdentityDoc;
using wfc.referential.Application.CountryIdentityDocs.Dtos;
using wfc.referential.Application.CountryIdentityDocs.Queries.GetCountryIdentityDocById;
using wfc.referential.Application.CountryIdentityDocs.Queries.GetFiltredCountryIdentityDocs;

namespace wfc.referential.API.Endpoints;

public static class CountryIdentityDocEndpoints
{
    public static void MapCountryIdentityDocEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/countryidentitydocs")
            .WithTags("CountryIdentityDocs");

        group.MapPost("/", CreateCountryIdentityDoc)
            .WithName("CreateCountryIdentityDoc")
            .WithSummary("Create a new Country-Identity Document association")
            .WithDescription("Creates an association between a Country and an Identity Document.")
            .Produces<Guid>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapGet("/", GetFiltredCountryIdentityDocs)
            .WithName("GetFiltredCountryIdentityDocs")
            .WithSummary("Get paginated list of country-identity document associations")
            .WithDescription("Returns a paginated list of country-identity document associations. Filters supported: countryId, identityDocumentId, status.")
            .Produces<PagedResult<GetCountryIdentityDocsResponse>>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapGet("/{countryIdentityDocId:guid}", GetCountryIdentityDocById)
            .WithName("GetCountryIdentityDocById")
            .WithSummary("Get a CountryIdentityDoc by GUID")
            .WithDescription("Retrieves the CountryIdentityDoc identified by countryIdentityDocId.")
            .Produces<GetCountryIdentityDocsResponse>(200)
            .Produces(404)
            .ProducesValidationProblem(400);

        group.MapPut("/{countryIdentityDocId:guid}", UpdateCountryIdentityDoc)
            .WithName("UpdateCountryIdentityDoc")
            .WithSummary("Update an existing Country-Identity Document association")
            .WithDescription("Updates the association identified by countryIdentityDocId with supplied body fields.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapPatch("/{countryIdentityDocId:guid}", PatchCountryIdentityDoc)
            .WithName("PatchCountryIdentityDoc")
            .WithSummary("Partially update a Country-Identity Document association")
            .WithDescription("Updates only the supplied fields for the association identified by countryIdentityDocId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409);

        group.MapDelete("/{countryIdentityDocId:guid}", DeleteCountryIdentityDoc)
            .WithName("DeleteCountryIdentityDoc")
            .WithSummary("Delete a country-identity document association by GUID")
            .WithDescription("Soft-deletes the association identified by countryIdentityDocId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404);
    }

    internal static async Task<IResult> CreateCountryIdentityDoc(
        CreateCountryIdentityDocRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreateCountryIdentityDocCommand>();
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> GetFiltredCountryIdentityDocs(
        [AsParameters] GetFiltredCountryIdentityDocsRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFiltredCountryIdentityDocsQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> GetCountryIdentityDocById(
        Guid countryIdentityDocId,
        IMediator mediator)
    {
        var query = new GetCountryIdentityDocByIdQuery { CountryIdentityDocId = countryIdentityDocId };
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdateCountryIdentityDoc(
        Guid countryIdentityDocId,
        UpdateCountryIdentityDocRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdateCountryIdentityDocCommand>();
        command = command with { CountryIdentityDocId = countryIdentityDocId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchCountryIdentityDoc(
        Guid countryIdentityDocId,
        PatchCountryIdentityDocRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchCountryIdentityDocCommand>();
        command = command with { CountryIdentityDocId = countryIdentityDocId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeleteCountryIdentityDoc(
        Guid countryIdentityDocId,
        IMediator mediator)
    {
        var command = new DeleteCountryIdentityDocCommand { CountryIdentityDocId = countryIdentityDocId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}