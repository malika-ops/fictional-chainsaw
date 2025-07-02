using BuildingBlocks.Core.Pagination;
using Mapster;
using MediatR;
using wfc.referential.Application.ServiceControles.Commands.CreateServiceControle;
using wfc.referential.Application.ServiceControles.Commands.DeleteServiceControle;
using wfc.referential.Application.ServiceControles.Commands.PatchServiceControle;
using wfc.referential.Application.ServiceControles.Commands.UpdateServiceControle;
using wfc.referential.Application.ServiceControles.Dtos;
using wfc.referential.Application.ServiceControles.Queries.GetFiltredServiceControles;
using wfc.referential.Application.ServiceControles.Queries.GetServiceControleById;

namespace wfc.referential.API.Endpoints;

public static class ServiceControleEndpoints
{
    public static void MapServiceControleEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/serviceControles")
                       .WithTags("ServiceControles");

        group.MapPost("/", CreateServiceControle)
             .WithName("CreateServiceControle")
             .WithSummary("Create a new Service-Controle link")
             .WithDescription("Creates the association between a Service, a Controle and a Channel with an ExecOrder. The trio (Service, Controle, Channel) must be unique.")
             .Produces<Guid>(201)
             .ProducesValidationProblem(400)
             .ProducesProblem(409)
             .ProducesProblem(500);

        group.MapGet("/", GetFiltredServiceControles)
             .WithName("GetFiltredServiceControles")
             .WithSummary("Get paginated list of Service-Controle mappings")
             .WithDescription("Returns a paginated list. Optional filters: ServiceId, ControleId, ChannelId, IsEnabled.")
             .Produces<PagedResult<ServiceControleResponse>>(200)
             .ProducesValidationProblem(400)
             .ProducesProblem(500);

        group.MapGet("/{serviceControleId:guid}", GetServiceControleById)
             .WithName("GetServiceControleById")
             .WithSummary("Get a Service-Controle link by GUID")
             .WithDescription("Retrieves the Service-Controle identified by serviceControleId.")
             .Produces<ServiceControleResponse>(200)
             .Produces(404)
             .ProducesValidationProblem(400);

        group.MapPut("/{serviceControleId:guid}", UpdateServiceControle)
             .WithName("UpdateServiceControle")
             .WithSummary("Update an existing Service-Controle link")
             .WithDescription("Replaces ServiceId, ControleId, ChannelId, ExecOrder and status.")
             .Produces<bool>(200)
             .ProducesValidationProblem(400)
             .Produces(404)
             .ProducesProblem(409);

        group.MapPatch("/{serviceControleId:guid}", PatchServiceControle)
             .WithName("PatchServiceControle")
             .WithSummary("Partially update a Service-Controle link")
             .WithDescription("Updates only the supplied fields (ServiceId, ControleId, ChannelId, ExecOrder, IsEnabled).")
             .Produces<bool>(200)
             .ProducesValidationProblem(400)
             .Produces(404)
             .ProducesProblem(409);

        group.MapDelete("/{serviceControleId:guid}", DeleteServiceControle)
             .WithName("DeleteServiceControle")
             .WithSummary("Delete a Service-Controle mapping by GUID")
             .WithDescription("Soft-deletes the link by disabling it (IsEnabled = false).")
             .Produces<bool>(200)
             .ProducesValidationProblem(400);
    }


    internal static async Task<IResult> CreateServiceControle(
        CreateServiceControleRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<CreateServiceControleCommand>();
        var result = await mediator.Send(command);
        return Results.Created($"/api/serviceControles/{result.Value}", result.Value);
    }

    internal static async Task<IResult> GetFiltredServiceControles(
        [AsParameters] GetFiltredServiceControlesRequest request,
        IMediator mediator)
    {
        var query = request.Adapt<GetFiltredServiceControlesQuery>();
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> GetServiceControleById(
        Guid serviceControleId,
        IMediator mediator)
    {
        var query = new GetServiceControleByIdQuery { ServiceControleId = serviceControleId };
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    internal static async Task<IResult> UpdateServiceControle(
        Guid serviceControleId,
        UpdateServiceControleRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<UpdateServiceControleCommand>() with { ServiceControleId = serviceControleId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> PatchServiceControle(
        Guid serviceControleId,
        PatchServiceControleRequest request,
        IMediator mediator)
    {
        var command = request.Adapt<PatchServiceControleCommand>() with { ServiceControleId = serviceControleId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }

    internal static async Task<IResult> DeleteServiceControle(
        Guid serviceControleId,
        IMediator mediator)
    {
        var command = new DeleteServiceControleCommand { ServiceControleId = serviceControleId };
        var result = await mediator.Send(command);
        return Results.Ok(result.Value);
    }
}
