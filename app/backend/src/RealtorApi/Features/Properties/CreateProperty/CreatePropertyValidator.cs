using FluentValidation;

namespace RealtorApi.Features.Properties.CreateProperty;

public sealed class CreatePropertyValidator : AbstractValidator<CreatePropertyRequest>
{
    public CreatePropertyValidator()
    {
        RuleFor(request => request.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.Description)
            .NotEmpty();

        RuleFor(request => request.Address)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(request => request.Price)
            .NotNull()
            .GreaterThan(0);

        RuleFor(request => request.Status)
            .Must(status => status is "Available" or "Rented" or "Maintenance")
            .WithMessage("El estado debe ser Available, Rented o Maintenance.");

        RuleFor(request => request.BedroomCount)
            .NotNull()
            .GreaterThanOrEqualTo(0);

        RuleFor(request => request.BathroomCount)
            .NotNull()
            .GreaterThanOrEqualTo(0);

        RuleFor(request => request.AreaSquareMeters)
            .NotNull()
            .GreaterThan(0);
    }
}
