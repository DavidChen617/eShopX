namespace ApplicationCore.UseCases.Users.UploadAvatar;

public class UploadUserAvatarCommandValidator : AbstractValidator<UploadUserAvatarCommand>
{
    public UploadUserAvatarCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.Image)
            .NotEmpty().WithMessage("Image is required");

        RuleFor(x => x.Image.FileName)
            .NotEmpty().WithMessage("FileName is required");

        RuleFor(x => x.Image.ContentType)
            .NotEmpty().WithMessage("ContentType is required");

        RuleFor(x => x.Image.Length)
            .GreaterThan(0).WithMessage("Image length must be greater than 0");

        RuleFor(x => x.Image.Content)
            .NotEmpty().WithMessage("Image content is required");
    }
}
