using FluentValidation;


namespace wfc.referential.Application.Regions.Commands.DeleteRegion;

public class DeleteRegionCommandValidator : AbstractValidator<DeleteRegionCommand>
{

    public DeleteRegionCommandValidator()
    {
        RuleFor(x => x.RegionId)
            .NotEmpty().WithMessage("RegionId is required.");
    }
}
