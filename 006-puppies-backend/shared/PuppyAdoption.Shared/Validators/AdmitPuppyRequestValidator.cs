using FluentValidation;
using NodaTime;

namespace PuppyAdoption.Shared.Validators;

/// <summary>
///     Validation for admitting a new puppy to a shelter
/// </summary>
public sealed class AdmitPuppyRequestValidator : AbstractValidator<AdmitPuppyRequest>
{
    /// <summary>
    ///     Maximum size of the (already resized and encoded) image in bytes
    /// </summary>
    public const int MaxImageBytes = 1024 * 1024;

    /// <summary>
    ///     Maximum length of the name and breed
    /// </summary>
    public const int MaxTextLength = 100;

    /// <param name="clock">Clock used to determine the current date</param>
    /// <param name="timeZone">Time zone in which "today" is determined</param>
    public AdmitPuppyRequestValidator(IClock clock, DateTimeZone timeZone)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(MaxTextLength);

        RuleFor(x => x.Breed)
            .NotEmpty()
            .MaximumLength(MaxTextLength);

        RuleFor(x => x.DateOfBirth)
            .Must(dateOfBirth => dateOfBirth <= Today(clock, timeZone))
            .WithMessage("must not be in the future");

        When(x => x.ImageData is not null, () =>
        {
            RuleFor(x => x.ImageData)
                .Must(image => image is { Length: > 0 and <= MaxImageBytes })
                .WithMessage($"must not be empty and not larger than {MaxImageBytes} bytes");
        });
    }

    private static LocalDate Today(IClock clock, DateTimeZone timeZone) => clock.GetCurrentInstant().InZone(timeZone).Date;
}