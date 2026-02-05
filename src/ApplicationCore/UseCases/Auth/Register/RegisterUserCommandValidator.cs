namespace ApplicationCore.UseCases.Auth.Register;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaxLength(100).WithMessage("Name must be at most 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .Email().WithMessage("Email format is invalid");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .Matches(@"^09\d{8}$").WithMessage("Phone format is invalid. It should be a 10-digit number starting with 09.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinLength(8).WithMessage("Password must be at least 8 characters");
    }
}