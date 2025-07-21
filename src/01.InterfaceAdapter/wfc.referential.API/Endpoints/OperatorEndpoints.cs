using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.Operators.Commands.CreateOperator;
using wfc.referential.Application.Operators.Commands.DeleteOperator;
using wfc.referential.Application.Operators.Commands.PatchOperator;
using wfc.referential.Application.Operators.Commands.UpdateOperator;
using wfc.referential.Application.Operators.Dtos;
using wfc.referential.Application.Operators.Queries.GetOperatorById;
using wfc.referential.Application.Operators.Queries.GetFiltredOperators;

namespace wfc.referential.API.Endpoints;

public static class OperatorEndpoints
{
    public static void MapOperatorEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/operators")
            .WithTags("Operators");

        group.MapPost("/", CreateOperator)
            .WithName("CreateOperator")
            .WithSummary("Create a new Operator")
            .WithDescription("Creates an operator with code, identity code, personal information, and other relevant details.")
            .Produces<Guid>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapGet("/", GetFiltredOperators)
            .WithName("GetFiltredOperators")
            .WithSummary("Get paginated list of operators")
            .WithDescription("Returns a paginated list of operators. Filters supported: code, identity code, name, email, operator type, branch ID, and enabled status.")
            .Produces<PagedResult<GetOperatorsResponse>>(200)
            .ProducesValidationProblem(400)
            .ProducesProblem(500);

        group.MapGet("/{operatorId:guid}", GetOperatorById)
            .WithName("GetOperatorById")
            .WithSummary("Get an Operator by GUID")
            .WithDescription("Retrieves the Operator identified by operatorId.")
            .Produces<GetOperatorsResponse>(200)
            .Produces(404)
            .ProducesValidationProblem(400);

        group.MapPut("/{operatorId:guid}", UpdateOperator)
            .WithName("UpdateOperator")
            .WithSummary("Update an existing Operator")
            .WithDescription("Updates the operator identified by operatorId with supplied body fields.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409)
            .ProducesProblem(500);

        group.MapPatch("/{operatorId:guid}", PatchOperator)
            .WithName("PatchOperator")
            .WithSummary("Partially update an Operator")
            .WithDescription("Updates only the supplied fields for the operator identified by operatorId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409);

        group.MapDelete("/{operatorId:guid}", DeleteOperator)
            .WithName("DeleteOperator")
            .WithSummary("Delete an operator by GUID")
            .WithDescription("Soft-deletes the operator identified by operatorId.")
            .Produces<bool>(200)
            .ProducesValidationProblem(400)
            .Produces(404)
            .ProducesProblem(409);
    }

    internal static async Task<IResult> CreateOperator(
        CreateOperatorRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreateOperatorCommand>();
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> GetFiltredOperators(
        [AsParameters] GetFiltredOperatorsRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFiltredOperatorsQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> GetOperatorById(
        Guid operatorId,
        IMediator mediator)
    {
        var query = new GetOperatorByIdQuery { OperatorId = operatorId };
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdateOperator(
        Guid operatorId,
        UpdateOperatorRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdateOperatorCommand>();
        command = command with { OperatorId = operatorId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchOperator(
        Guid operatorId,
        PatchOperatorRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchOperatorCommand>();
        command = command with { OperatorId = operatorId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeleteOperator(
        Guid operatorId,
        IMediator mediator)
    {
        var command = new DeleteOperatorCommand { OperatorId = operatorId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}