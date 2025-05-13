using FluentValidation;


namespace wfc.referential.Application.Cities.Commands.DeleteCity;

public class DeleteCityCommandValidator : AbstractValidator<DeleteCityCommand>
{

    public DeleteCityCommandValidator()
    {
        RuleFor(x => x.CityId)
            .NotEmpty().WithMessage(c => $"{c.CityId} is required.");
    }
}
