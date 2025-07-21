using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.OperatorAggregate;
using wfc.referential.Domain.OperatorAggregate.Exceptions;
using wfc.referential.Domain.AgencyAggregate;

namespace wfc.referential.Application.Operators.Commands.PatchOperator;

public class PatchOperatorCommandHandler : ICommandHandler<PatchOperatorCommand, Result<bool>>
{
    private readonly IOperatorRepository _repo;
    private readonly IAgencyRepository _agencyRepository;

    public PatchOperatorCommandHandler(
        IOperatorRepository repo,
        IAgencyRepository agencyRepository)
    {
        _repo = repo;
        _agencyRepository = agencyRepository;
    }

    public async Task<Result<bool>> Handle(PatchOperatorCommand cmd, CancellationToken ct)
    {
        var operatorEntity = await _repo.GetByIdAsync(OperatorId.Of(cmd.OperatorId), ct);
        if (operatorEntity is null)
            throw new OperatorNotFoundException(cmd.OperatorId);

        // Check if code is unique (if being updated)
        if (cmd.Code != null && cmd.Code != operatorEntity.Code)
        {
            var existingByCode = await _repo.GetByConditionAsync(o => o.Code == cmd.Code, ct);
            if (existingByCode.Any())
                throw new OperatorCodeAlreadyExistException(cmd.Code);
        }

        // Check if identity code is unique (if being updated)
        if (cmd.IdentityCode != null && cmd.IdentityCode != operatorEntity.IdentityCode)
        {
            var existingByIdentityCode = await _repo.GetByConditionAsync(o => o.IdentityCode == cmd.IdentityCode, ct);
            if (existingByIdentityCode.Any())
                throw new ConflictException($"Operator with identity code {cmd.IdentityCode} already exists.");
        }

        // Check if email is unique (if being updated)
        if (cmd.Email != null && cmd.Email != operatorEntity.Email)
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

        // TODO: Validate ProfileId exists if provided
        // La table profile n'existe pas encore
        // if (cmd.ProfileId.HasValue)
        // {
        //     var profile = await _profileRepository.GetByIdAsync(ProfileId.Of(cmd.ProfileId.Value), ct);
        //     if (profile is null)
        //         throw new ResourceNotFoundException($"Profile with ID {cmd.ProfileId.Value} not found");
        // }

        operatorEntity.Patch(
            cmd.Code,
            cmd.IdentityCode,
            cmd.LastName,
            cmd.FirstName,
            cmd.Email,
            cmd.PhoneNumber,
            cmd.IsEnabled,
            cmd.OperatorType,
            cmd.BranchId);

        await _repo.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}
