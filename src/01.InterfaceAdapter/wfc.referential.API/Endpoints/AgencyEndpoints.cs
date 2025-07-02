using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.Agencies.Commands.CreateAgency;
using wfc.referential.Application.Agencies.Commands.DeleteAgency;
using wfc.referential.Application.Agencies.Commands.PatchAgency;
using wfc.referential.Application.Agencies.Commands.UpdateAgency;
using wfc.referential.Application.Agencies.Dtos;
using wfc.referential.Application.Agencies.Queries.GetAgencyById;
using wfc.referential.Application.Agencies.Queries.GetFiltredAgencies;

namespace wfc.referential.API.Endpoints;

public static class AgencyEndpoints
{
    public static void MapAgencyEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/agencies")
            .WithTags("Agencies");

        group.MapPost("/", CreateAgency)
            .WithName("CreateAgency")
            .WithSummary("Create a new Agency")
            .WithDescription("Creates an agency with the provided code, label, address, phone, accounting data, geo‑location, etc.")
            .Produces<Guid>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapGet("/", GetFiltredAgencies)
            .WithName("GetFiltredAgencies")
            .WithSummary("Get paginated list of agencies")
            .WithDescription("Returns a paginated list of agencies. Filters supported: code, name, abbreviation, phone, fax, accounting fields, postal code, CityId, SectorId, AgencyType (id, value, libellé), status.")
            .Produces<PagedResult<GetAgenciesResponse>>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapGet("/{agencyId:guid}", GetAgencyById)
            .WithName("GetAgencyById")
            .WithSummary("Get an Agency by GUID")
            .WithDescription("Retrieves the Agency identified by agencyId.")
            .Produces<GetAgenciesResponse>(200)
            .Produces(404)
            .ProducesValidationProblem(400);

        group.MapPut("/{agencyId:guid}", UpdateAgency)
            .WithName("UpdateAgency")
            .WithSummary("Update an existing Agency")
            .WithDescription("Updates the agency identified by agencyId with supplied body fields.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapPatch("/{agencyId:guid}", PatchAgency)
            .WithName("PatchAgency")
            .WithSummary("PartiFiltredy update an Agency")
            .WithDescription("Updates only the supplied fields for the agency identified by agencyId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409);

        group.MapDelete("/{agencyId:guid}", DeleteAgency)
            .WithName("DeleteAgency")
            .WithSummary("Delete an agency by GUID")
            .WithDescription("Soft-deletes the agency identified by agencyId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400);
    }

    internal static async Task<IResult> CreateAgency(
        CreateAgencyRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreateAgencyCommand>();
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> GetFiltredAgencies(
        [AsParameters] GetFiltredAgenciesRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFiltredAgenciesQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> GetAgencyById(
        Guid agencyId,
        IMediator mediator)
    {
        var query = new GetAgencyByIdQuery { AgencyId = agencyId };
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdateAgency(
        Guid agencyId,
        UpdateAgencyRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdateAgencyCommand>();
        command = command with { AgencyId = agencyId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchAgency(
        Guid agencyId,
        PatchAgencyRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchAgencyCommand>();
        command = command with { AgencyId = agencyId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeleteAgency(
        Guid agencyId,
        IMediator mediator)
    {
        var command = new DeleteAgencyCommand { AgencyId = agencyId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}