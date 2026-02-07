namespace eShopX.Common.Validation;

public class ValidationResult
{
    public List<ValidationFailure> Errors { get; set; } = new();
    public bool IsValid => Errors.Count == 0;
}
