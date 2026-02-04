namespace ApplicationCore.UseCases.Users.DeleteAvatar;

public class DeleteUserAvatarCommandValidator : AbstractValidator<DeleteUserAvatarCommand>
{
    public DeleteUserAvatarCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull().WithMessage("UserId is required")
            .NotEmpty().WithMessage("UserId is required");
    }
}
