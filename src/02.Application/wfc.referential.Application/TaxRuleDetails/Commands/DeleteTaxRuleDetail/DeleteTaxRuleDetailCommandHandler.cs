using BuildingBlocks.Core.Abstraction.CQRS;
using BuildingBlocks.Core.Abstraction.Domain;
using BuildingBlocks.Core.Exceptions;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.TaxRuleDetailAggregate;

namespace wfc.referential.Application.TaxRuleDetails.Commands.DeleteTaxRuleDetail;

public class DeleteTaxRuleDetailCommandHandler(
        ITaxRuleDetailRepository _taxRuleDetailsRepository) : ICommandHandler<DeleteTaxRuleDetailCommand, Result<bool>>
{

    public async Task<Result<bool>> Handle(DeleteTaxRuleDetailCommand request, CancellationToken cancellationToken)
    {
        var taxRuleDetailsId = TaxRuleDetailsId.Of(request.TaxRuleDetailId);
        var taxRuleDetail = await _taxRuleDetailsRepository.GetByIdAsync(taxRuleDetailsId, cancellationToken);
        if (taxRuleDetail is null)
            throw new ResourceNotFoundException($"{nameof(TaxRuleDetail)} not found");

        taxRuleDetail.SetInactive();

        _taxRuleDetailsRepository.Update(taxRuleDetail);
        await _taxRuleDetailsRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}