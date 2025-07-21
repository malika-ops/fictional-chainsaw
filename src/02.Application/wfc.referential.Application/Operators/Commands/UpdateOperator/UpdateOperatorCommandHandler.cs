using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.OperatorAggregate;
using wfc.referential.Domain.OperatorAggregate.Exceptions;
using wfc.referential.Domain.AgencyAggregate;

namespace wfc.referential.Application.Operators.Commands.UpdateOperator;

public class UpdateOperatorCommandHandler : ICommandHandler<UpdateOperatorCommand, Result<bool>>
{
    private readonly IOperatorRepository _repo;
    private readonly IAgencyRepository _agencyRepository;

    public UpdateOperatorCommandHandler(
        IOperatorRepository repo,
        IAgencyRepository agencyRepository)
    {
        _repo = repo;
        _agencyRepository = agencyRepository;
    }

    public async Task<Result<bool>> Handle(UpdateOperatorCommand cmd, CancellationToken ct)
    {
        var operatorEntity = await _repo.GetByIdAsync(OperatorId.Of(cmd.OperatorId), ct);
        if (operatorEntity is null)
            throw new OperatorNotFoundException(cmd.OperatorId);

        // Check if code is unique (if changed)
        if (cmd.Code != operatorEntity.Code)
        {
            var existingByCode = await _repo.GetByConditionAsync(o => o.Code == cmd.Code, ct);
            if (existingByCode.Any())
                throw new OperatorCodeAlreadyExistException(cmd.Code);
        }

        // Check if identity code is unique (if changed)
        if (cmd.IdentityCode != operatorEntity.IdentityCode)
        {
            var existingByIdentityCode = await _repo.GetByConditionAsync(o => o.IdentityCode == cmd.IdentityCode, ct);
            if (existingByIdentityCode.Any())
                throw new ConflictException($"Operator with identity code {cmd.IdentityCode} already exists.");
        }

        // Check if email is unique (if changed)
        if (cmd.Email != operatorEntity.Email)
        {
            var existingByEmail = await _repo.GetByConditionAsync(o => o.Email == cmd.Email, ct);
            if (existingByEmail.Any())
                throw new ConflictException($"Operator with email {cmd.Email} already exists.");
        }

        // Validate Branch exists if provided
        if (cmd.BranchId.HasValue)
        {
            var branch = await _agencyRepository.GetByIdAsync(AgencyId.Of(cmd.BranchId.Value), ct);
            if (branch is null)
                throw new ResourceNotFoundException($"Branch with ID {cmd.BranchId.Value} not found");
        }

        operatorEntity.Update(
            cmd.Code,
            cmd.IdentityCode,
            cmd.LastName,
            cmd.FirstName,
            cmd.Email,
            cmd.PhoneNumber,
            cmd.IsEnabled,
            cmd.OperatorType,
            cmd.BranchId,
            cmd.ProfileId);

        await _repo.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}