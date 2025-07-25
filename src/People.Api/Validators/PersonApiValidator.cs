using FluentValidation;
using People.Api.ApiModels;

namespace People.Api.Validators;

public class PersonApiValidator : AbstractValidator<PersonApi>
{
    public PersonApiValidator()
    {
        RuleFor(person => person.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
        RuleFor(person => person.DateOfBirth)
            .NotEmpty().WithMessage("Date of Birth is required.")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage("Date of Birth must be in the past.");
    }
}