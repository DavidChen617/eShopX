namespace eShopX.Common.Validation;

public class PropertyRule<T, TProperty>(string propertyName, Func<T, TProperty> propertyFunc)
    : IPropertyRule<T>
{
    public List<Func<T, TProperty, CancellationToken, Task<string?>>> Rules { get; } = new();
    public Func<T, bool>? Condition { get; set; }
    public string PropertyName => propertyName;

    public async Task<List<ValidationFailure>> ValidateAsync(T instance, CancellationToken ct = default)
    {
        var failures = new List<ValidationFailure>();
        if (Condition is not null && !Condition(instance))
            return failures;

        var value = propertyFunc(instance);

        foreach (var rule in Rules)
        {
            var errorMsg = await (rule(instance, value, ct));
            if (errorMsg is not null)
                failures.Add(new ValidationFailure(propertyName, errorMsg));
        }

        return failures;
    }
}