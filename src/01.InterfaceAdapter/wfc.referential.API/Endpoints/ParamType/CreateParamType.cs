using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.ParamTypes.Commands.CreateParamType;
using wfc.referential.Application.ParamTypes.Dtos;

namespace wfc.referential.API.Endpoints.ParamType;

public class CreateParamType(IMediator _mediator) : Endpoint<CreateParamTypeRequest, Guid>
{
    public override void Configure()
    {
        Post("/api/paramtypes");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new ParamType";
            s.Description = "Creates a ParamType with value and type definition. Value must be unique for the type definition.";

            s.Response<Guid>(200, "Returns the ID of the newly created ParamType if successful");
            s.Response(400, "Returned if validation fails (missing fields or duplicate value)");
            s.Response(500, "Server error if an unexpected exception occurs");
        });

        Options(o => o.WithTags(EndpointGroups.ParamTypes));
    }

    public override async Task HandleAsync(CreateParamTypeRequest dto, CancellationToken ct)
    {
        var command = dto.Adapt<CreateParamTypeCommand>();
        var result = await _mediator.Send(command, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}