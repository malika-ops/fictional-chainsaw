using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Audit.Interface;
using BuildingBlocks.Core.Exceptions;
using BuildingBlocks.Core.Kafka.Producer;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.MonetaryZone.Events;
using wfc.referential.Domain.MonetaryZoneAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate.Exceptions;

namespace wfc.referential.Application.MonetaryZones.Commands.PatchMonetaryZone;

public class PatchMonetaryZoneCommandHandler(
    IMonetaryZoneRepository monetaryZoneRepository,
    IProducerService kafkaProducer,
    ICurrentUserContext userContext) 
    : ICommandHandler<PatchMonetaryZoneCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(PatchMonetaryZoneCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId ?? "anonymous";
        var traceId = userContext.TraceId ?? Guid.NewGuid().ToString();

        var monetaryZoneId = MonetaryZoneId.Of(request.MonetaryZoneId);

        var monetaryZone = await monetaryZoneRepository.GetByIdAsync(monetaryZoneId, cancellationToken) 
            ?? throw new BusinessException("Monetary zone not found");

        if (!string.IsNullOrEmpty(request.Code))
        {
            var isExist = await monetaryZoneRepository.GetOneByConditionAsync(m=>m.Code == request.Code, cancellationToken);

            if (isExist is not null && !string.Equals(isExist.Code, monetaryZone!.Code, StringComparison.OrdinalIgnoreCase))
                throw new CodeAlreadyExistException(request.Code);
        }

        // Create audit copy BEFORE modification using MonetaryZone.Create
        var monetaryZoneBeforeUpdate = MonetaryZone.Create(
            monetaryZone.Id,
            monetaryZone.Code,
            monetaryZone.Name,
            monetaryZone.Description);

        monetaryZone.Patch(request.Code , request.Name , request.Description , request.IsEnabled);

        await monetaryZoneRepository.SaveChangesAsync(cancellationToken);

        // Create patch-specific audit event
        var auditEvent = new MonetaryZonePatchedAuditEvent
        {
            NewValueJson = monetaryZone,
            OldValueJson = monetaryZoneBeforeUpdate,
            Timestamp = DateTime.UtcNow,
            EntityId = monetaryZone.Id!.ToString(),
            MetadataJson = new
            {
                ip = userContext.IpAddress,
                TraceId = traceId
            }
        };

        await kafkaProducer.ProduceAsync(auditEvent);

        return Result.Success(true);
    }
}
