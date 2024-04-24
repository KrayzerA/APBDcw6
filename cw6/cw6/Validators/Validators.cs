using FluentValidation;

namespace cw6.Validators;

public static class Validators
{
    public static void RegisterValidators(this IServiceCollection service)
    {
        service.AddValidatorsFromAssemblyContaining<AnimalCreateRequestValidator>();
        service.AddValidatorsFromAssemblyContaining<AnimalReplaceRequestValidator>();
    }
}