namespace ApplicationCore.UseCases.Users.UpdateMe;

public class UpdateMeValidator : AbstractValidator<UpdateMeCommand>
{
    public UpdateMeValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaxLength(100).WithMessage("Name must be at most 100 characters");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .Matches(@"^09\d{8}$").WithMessage("Phone format is invalid. It should be a 10-digit number starting with 09.");

        RuleFor(x => x.Address)
            .MaxLength(200).WithMessage("Address must be at most 200 characters")
            .When(x => x.Address is not null);
    }
}
