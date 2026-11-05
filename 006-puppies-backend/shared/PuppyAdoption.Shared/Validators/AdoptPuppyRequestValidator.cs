using FluentValidation;
namespace PuppyAdoption.Shared.Validators;

/// <summary>
///     Validation for adopting a puppy
/// </summary>
public sealed class AdoptPuppyRequestValidator : AbstractValidator<AdoptPuppyRequest>
{
    /// <summary>
    ///     Maximum length of the adopter's name
    /// </summary>
    public const int MaxNameLength = 100;

    public AdoptPuppyRequestValidator()
    {
        RuleFor(x => x.AdopterName)
            .NotEmpty()
            .MaximumLength(MaxNameLength);
    }
}
