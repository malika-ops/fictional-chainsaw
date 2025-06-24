using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.ParamTypes.Commands.CreateParamType;
using wfc.referential.Application.ParamTypes.Commands.DeleteParamType;
using wfc.referential.Application.ParamTypes.Commands.PatchParamType;
using wfc.referential.Application.ParamTypes.Commands.UpdateParamType;
using wfc.referential.Application.ParamTypes.Dtos;
using wfc.referential.Application.ParamTypes.Queries.GetFiltredParamTypes;

namespace wfc.referential.API.Features;

public static class ParamTypeEndpoints
{
    public static void MapParamTypeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/paramtypes")
            .WithTags("ParamTypes");

        group.MapPost("/", CreateParamType)
            .WithName("CreateParamType")
            .WithSummary("Create a new ParamType")
            .WithDescription("Creates a ParamType with the provided value and type definition.")
            .Produces<Guid>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapGet("/", GetFiltredParamTypes)
            .WithName("GetFiltredParamTypes")
            .WithSummary("Get paginated list of ParamTypes")
            .WithDescription("Returns a paginated list of ParamTypes. Filters supported: value, typeDefinitionId, status.")
            .Produces<PagedResult<GetFiltredParamTypesResponse>>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapPut("/{paramTypeId:guid}", UpdateParamType)
            .WithName("UpdateParamType")
            .WithSummary("Update an existing ParamType")
            .WithDescription("Updates the ParamType identified by paramTypeId with supplied body fields.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapPatch("/{paramTypeId:guid}", PatchParamType)
            .WithName("PatchParamType")
            .WithSummary("Partially update a ParamType")
            .WithDescription("Updates only the supplied fields for the ParamType identified by paramTypeId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409);

        group.MapDelete("/{paramTypeId:guid}", DeleteParamType)
            .WithName("DeleteParamType")
            .WithSummary("Delete a ParamType by GUID")
            .WithDescription("Soft-deletes the ParamType identified by paramTypeId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409);
    }

    internal static async Task<IResult> CreateParamType(
        CreateParamTypeRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreateParamTypeCommand>();
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> GetFiltredParamTypes(
        [AsParameters] GetFiltredParamTypesRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFiltredParamTypesQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdateParamType(
        Guid paramTypeId,
        UpdateParamTypeRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdateParamTypeCommand>();
        command = command with { ParamTypeId = paramTypeId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchParamType(
        Guid paramTypeId,
        PatchParamTypeRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchParamTypeCommand>();
        command = command with { ParamTypeId = paramTypeId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeleteParamType(
        Guid paramTypeId,
        IMediator mediator)
    {
        var command = new DeleteParamTypeCommand(paramTypeId);
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}