using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.ParamTypes.Commands.DeleteParamType;
using wfc.referential.Application.ParamTypes.Dtos;

namespace wfc.referential.API.Endpoints.ParamType;

public class DeleteParamType(IMediator _mediator) : Endpoint<DeleteParamTypeRequest, bool>
{
    public override void Configure()
    {
        Delete("/api/paramtypes/{ParamTypeId}");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Delete a ParamType by GUID";
            s.Description = "Deletes the ParamType identified by {ParamTypeId}, as route param.";

            s.Response<bool>(200, "True if deletion succeeded");
            s.Response(400, "If deletion failed due to validation errors");
            s.Response(404, "If paramtype was not found");
            s.Response(409, "If paramtype is in use and cannot be deleted");
        });

        Options(o => o.WithTags(EndpointGroups.ParamTypes));
    }

    public override async Task HandleAsync(DeleteParamTypeRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<DeleteParamTypeCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}