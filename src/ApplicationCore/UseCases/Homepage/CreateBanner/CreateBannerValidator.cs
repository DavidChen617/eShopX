namespace ApplicationCore.UseCases.Homepage.CreateBanner;

public class CreateBannerValidator : AbstractValidator<CreateBannerCommand>
{
    public CreateBannerValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("標題不能為空")
            .MaxLength(100).WithMessage("標題最多 100 字");

        RuleFor(x => x.ImageUrl)
            .NotEmpty().WithMessage("圖片網址不能為空")
            .MaxLength(500).WithMessage("圖片網址最多 500 字");

        RuleFor(x => x.Link)
            .NotEmpty().WithMessage("連結不能為空")
            .MaxLength(500).WithMessage("連結最多 500 字");
    }
}
