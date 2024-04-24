using cw6.DTOs;
using FluentValidation;

namespace cw6.Validators;

public class AnimalReplaceRequestValidator : AbstractValidator<ReplaceAnimalRequest>
{
    public AnimalReplaceRequestValidator()
    {
        RuleFor(a => a.Description).MaximumLength(200);
        RuleFor(a => a.Category).MaximumLength(200).NotNull();
        RuleFor(a => a.Area).MaximumLength(200).NotNull();
        RuleFor(a => a.Name).MaximumLength(200).NotNull();
    }
}