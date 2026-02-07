using System.Linq.Expressions;

namespace eShopX.Common.Validation;

public abstract class AbstractValidator<T> : IValidator<T>
{
    private readonly List<IPropertyRule<T>> _rules = new();

    public IRuleBuilder<T, TProperty> RuleFor<TProperty>(Expression<Func<T, TProperty>> expression)
    {
        var propertyName = GetPropertyName(expression);
        var propertyFunc = expression.Compile();
        var rule = new PropertyRule<T, TProperty>(propertyName, propertyFunc);
        var builder = new RuleBuilder<T, TProperty>(rule);
        _rules.Add(rule);

        return builder;
    }

    private static string GetPropertyName<TProperty>(Expression<Func<T, TProperty>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
            return memberExpression.Member.Name;
        if (expression.Body is UnaryExpression { Operand: MemberExpression operand })
            return operand.Member.Name;

        throw new ArgumentException("Invalid expression", nameof(expression));
    }

    public ValidationResult Validate(T instance)
    {
        return ValidateAsync(instance).GetAwaiter().GetResult();
    }

    public async Task<ValidationResult> ValidateAsync(T instance, CancellationToken ct = default)
    {
        var result = new ValidationResult();

        foreach (var rule in _rules)
        {
            var failures = await rule.ValidateAsync(instance!, ct);
            result.Errors.AddRange(failures);
        }

        return result;
    }
}
