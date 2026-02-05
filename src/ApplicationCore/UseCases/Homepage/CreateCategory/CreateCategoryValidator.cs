namespace ApplicationCore.UseCases.Homepage.CreateCategory;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("名稱不能為空")
            .MaxLength(50).WithMessage("名稱最多 50 字");

        RuleFor(x => x.Icon)
            .NotEmpty().WithMessage("圖標不能為空")
            .MaxLength(50).WithMessage("圖標最多 50 字");

        RuleFor(x => x.Link)
            .NotEmpty().WithMessage("連結不能為空")
            .MaxLength(200).WithMessage("連結最多 200 字");
    }
}