using FluentValidation;
using System.Globalization;

namespace RealtorApi.Features.Properties;

public sealed class GetPropertiesValidator : AbstractValidator<GetPropertiesRequest>
{
    public GetPropertiesValidator()
    {
        RuleFor(request => request.Page)
            .Must(IsIntegerOrMissing)
            .WithMessage("El parámetro page debe ser un número entero.")
            .DependentRules(() =>
            {
                RuleFor(request => request.Page)
                    .Must(page => page is null || int.Parse(page!, CultureInfo.InvariantCulture) > 0)
                    .WithMessage("El parámetro page debe ser mayor que 0.");
            });

        RuleFor(request => request.PageSize)
            .Must(IsIntegerOrMissing)
            .WithMessage("El parámetro pageSize debe ser un número entero.")
            .DependentRules(() =>
            {
                RuleFor(request => request.PageSize)
                    .Must(pageSize => pageSize is null || int.Parse(pageSize!, CultureInfo.InvariantCulture) is >= 1 and <= 100)
                    .WithMessage("El parámetro pageSize debe estar entre 1 y 100.");
            });
    }

    private static bool IsIntegerOrMissing(string? value)
    {
        return value is null || int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
    }
}
