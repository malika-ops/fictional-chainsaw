using FluentValidation;
using Microsoft.EntityFrameworkCore;
using wfc.referential.Application.Data;

namespace wfc.referential.Application.Regions.Commands.CreateRegion;


public class CreateRegionCommandValidator : AbstractValidator<CreateRegionCommand>
{

    public CreateRegionCommandValidator()
    {

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required");

        RuleFor(x => x.CountryId)
            .NotEmpty().WithMessage("Country Id is required");
    }

}