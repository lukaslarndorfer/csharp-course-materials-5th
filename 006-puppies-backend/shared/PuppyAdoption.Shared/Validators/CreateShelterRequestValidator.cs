using FluentValidation;
namespace PuppyAdoption.Shared.Validators;

public class CreateShelterRequestValidator : AbstractValidator<CreateShelterRequest>
{
    /// <summary>
    ///     Maximum length of the shelter's name
    /// </summary>
    public const int MaxNameLength = 100;

    public CreateShelterRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(MaxNameLength);
    }
}
