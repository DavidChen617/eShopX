namespace eShopX.Common.Validation;

public interface IPropertyRule<in T>
{
    Task<List<ValidationFailure>> ValidateAsync(T instance, CancellationToken ct = default);
}