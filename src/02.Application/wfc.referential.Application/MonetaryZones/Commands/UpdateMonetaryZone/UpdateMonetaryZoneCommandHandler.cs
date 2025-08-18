using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Audit.Interface;
using BuildingBlocks.Core.Exceptions;
using BuildingBlocks.Core.Kafka.Producer;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.MonetaryZone.Events;
using wfc.referential.Domain.MonetaryZoneAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate.Exceptions;

namespace wfc.referential.Application.MonetaryZones.Commands.UpdateMonetaryZone;

public class UpdateMonetaryZoneCommandHandler(
    IMonetaryZoneRepository monetaryZoneRepository,
    IProducerService kafkaProducer,
    ICurrentUserContext userContext)
    : ICommandHandler<UpdateMonetaryZoneCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateMonetaryZoneCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId ?? "anonymous";
        var traceId = userContext.TraceId ?? Guid.NewGuid().ToString();

        var monetaryZoneId = MonetaryZoneId.Of(request.MonetaryZoneId);
        var monetaryZone = await monetaryZoneRepository.GetByIdAsync(monetaryZoneId, cancellationToken)
            ?? throw new BusinessException("Monetary zone not found");

        // Verify if code exist for another MonetaryZone
        var existingZone = await monetaryZoneRepository.GetOneByConditionAsync(
            m => m.Code == request.Code,
            cancellationToken);

        if (existingZone is not null &&
            !string.Equals(existingZone.Code, monetaryZone.Code, StringComparison.OrdinalIgnoreCase))
        {
            throw new CodeAlreadyExistException(request.Code);
        }

        // Create copy MonetaryZone before Update
        var monetaryZoneBeforeUpdate = MonetaryZone.Create(
            monetaryZone.Id,
            monetaryZone.Code,
            monetaryZone.Name,
            monetaryZone.Description);

        // Update MonetaryZone
        monetaryZone.Update(request.Code, request.Name, request.Description, request.IsEnabled);

        await monetaryZoneRepository.SaveChangesAsync(cancellationToken);

        // Create Audit Event
        var auditEvent = new MonetaryZoneUpdatedAuditEvent
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