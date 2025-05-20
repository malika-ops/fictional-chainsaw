using FastEndpoints;
using Mapster;
using MediatR;
using wfc.referential.Application.AgencyTiers.Commands.CreateAgencyTier;
using wfc.referential.Application.AgencyTiers.Dtos;


namespace wfc.referential.API.Endpoints.AgencyTier;

public class CreateAgencyTier(IMediator _mediator)
    : Endpoint<CreateAgencyTierRequest, Guid>
{
    public override void Configure()
    {
        Post("/api/agencyTiers");
        AllowAnonymous();

        Summary(s =>
        {
            s.Summary = "Create a new AgencyTier link";
            s.Description = """
                            Creates the association between an Agency and a Tier, with a unique Code
                            and an optional Password. Code must be unique for the (Agency, Tier) pair.
                            """;
            s.Response<Guid>(200, "ID of the newly-created AgencyTier");
            s.Response(400, "Validation fails or duplicate code");
            s.Response(500, "Unexpected server error");
        });

        Options(o => o.WithTags(EndpointGroups.AgencyTiers));
    }

    public override async Task HandleAsync(CreateAgencyTierRequest dto, CancellationToken ct)
    {
        var cmd = dto.Adapt<CreateAgencyTierCommand>();
        var result = await _mediator.Send(cmd, ct);
        await SendAsync(result.Value, cancellation: ct);
    }
}