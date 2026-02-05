using System.Text.RegularExpressions;

namespace eShopX.Common.Validation;

public class RuleBuilder<T, TProperty>(PropertyRule<T, TProperty> rule) : IRuleBuilder<T, TProperty>
{
    private string? _customMessage;
    private bool _orNext;
    public PropertyRule<T, TProperty> Rule => rule;

    public IRuleBuilder<T, TProperty> AddRule(Func<TProperty, bool> predicate, string defaultMessage)
    {
        var msg = _customMessage ?? defaultMessage;
        _customMessage = null;
        AddRuleInternal((_, value, _) => Task.FromResult(predicate(value) ? null : msg));
        return this;
    }

    public IRuleBuilder<T, TProperty> AddRule(Func<T, TProperty, bool> predicate, string defaultMessage)
    {
        var msg = _customMessage ?? defaultMessage;
        _customMessage = null;
        AddRuleInternal((instance, value, _) => Task.FromResult(predicate(instance, value) ? null : msg));

        return this;
    }

    public IRuleBuilder<T, TProperty> AddAsyncRule(Func<TProperty, CancellationToken, Task<bool>> predicate, string defaultMessage)
    {
        var msg = _customMessage ?? defaultMessage;
        _customMessage = null;
        AddRuleInternal(async (_, value, ct) => await predicate(value, ct) ? null : msg);

        return this;
    }

    public IRuleBuilder<T, TProperty> NotNull()
    {
        return AddRule(value => value != null, $"{rule.PropertyName} must not be null");
    }

    public IRuleBuilder<T, TProperty> NotEmpty()
    {
        return AddRule(value =>
            {
                if (value is string str) return !string.IsNullOrWhiteSpace(str);
                if (value is System.Collections.IEnumerable enumerable)
                {
                    foreach (var _ in enumerable) return true;
                    return false;
                }
                return true;
            }, $"{rule.PropertyName} is required");
    }

    public IRuleBuilder<T, TProperty> MaxLength(int max)
    {
        return AddRule(value =>
            {
                if (value is not string str) return true;
                return str.Length <= max;
            }, $"{rule.PropertyName} must be at most {max} characters");
    }

    public IRuleBuilder<T, TProperty> MinLength(int min)
    {
        return AddRule(value =>
            {
                if (value is not string str) return true;
                return str.Length >= min;
            }, $"{rule.PropertyName} must be at least {min} characters");
    }

    public IRuleBuilder<T, TProperty> Email()
    {
        return AddRule(value =>
            {
                if (value is not string str) return true;
                if (string.IsNullOrWhiteSpace(str)) return true;
                return Regex.IsMatch(str, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            }, $"{rule.PropertyName} is not a valid email address");
    }

    public IRuleBuilder<T, TProperty> GreaterThan(TProperty compareValue)
    {
        return AddRule(value =>
            {
                if (value is IComparable comparable && compareValue is IComparable compareComparable)
                    return comparable.CompareTo(compareComparable) > 0;
                return true;
            }, $"{rule.PropertyName} must be greater than {compareValue}");
    }

    public IRuleBuilder<T, TProperty> LessThan(TProperty compareValue)
    {
        return AddRule(value =>
            {
                if (value is IComparable comparable && compareValue is IComparable compareComparable)
                    return comparable.CompareTo(compareComparable) < 0;
                return true;
            }, $"{rule.PropertyName} must be less than {compareValue}");
    }

    public IRuleBuilder<T, TProperty> Must(Func<TProperty, bool> predicate)
    {
        return AddRule(predicate, $"{rule.PropertyName} is invalid");
    }

    public IRuleBuilder<T, TProperty> MustAsync(Func<TProperty, CancellationToken, Task<bool>> predicate)
    {
        return AddAsyncRule(predicate, $"{rule.PropertyName} is invalid");
    }

    public IRuleBuilder<T, TProperty> Matches(string pattern)
    {
        return AddRule(value =>
            {
                if (value is not string str) return true;
                if (string.IsNullOrWhiteSpace(str)) return true;
                return Regex.IsMatch(str, pattern);
            }, $"{rule.PropertyName} format is invalid");
    }

    public IRuleBuilder<T, TProperty> InclusiveBetween(TProperty from, TProperty to)
    {
        return AddRule(value =>
            {
                if (value is IComparable comparable)
                {
                    var fromComparable = from as IComparable;
                    var toComparable = to as IComparable;
                    if (fromComparable == null || toComparable == null) return true;
                    return comparable.CompareTo(fromComparable) >= 0 && comparable.CompareTo(toComparable) <= 0;
                }
                return true;
            }, $"{rule.PropertyName} must be between {from} and {to}");
    }

    public IRuleBuilder<T, TProperty> IsInEnum()
    {
        return AddRule(value =>
            {
                if (value == null) return true;
                var type = typeof(TProperty);
                var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
                if (!underlyingType.IsEnum) return true;
                return Enum.IsDefined(underlyingType, value);
            }, $"{rule.PropertyName} is not a valid value");
    }

    public IRuleBuilder<T, TProperty> Equal(TProperty compareValue)
    {
        return AddRule(value2 => EqualityComparer<TProperty>.Default.Equals(value2, compareValue),
            $"{rule.PropertyName} must be equal to {compareValue}");
    }

    public IRuleBuilder<T, TProperty> Equal(Func<T, TProperty> selector)
    {
        return AddRule((instance, value) =>
            {
                var compareValue = selector(instance);
                return EqualityComparer<TProperty>.Default.Equals(value, compareValue);
            }, $"{rule.PropertyName} must match");
    }

    public IRuleBuilder<T, TProperty> NotEqual(TProperty compareValue)
    {
        return AddRule(value => !EqualityComparer<TProperty>.Default.Equals(value, compareValue),
            $"{rule.PropertyName} must not be equal to {compareValue}");
    }

    public IRuleBuilder<T, TProperty> WithMessage(string message)
    {
        if (rule.Rules.Count > 0)
        {
            var lastIndex = rule.Rules.Count - 1;
            var lastRule = rule.Rules[lastIndex];
            rule.Rules[lastIndex] = async (instance, value, ct) =>
            {
                var result = await lastRule(instance, value, ct);
                return result != null ? message : null;
            };
        }
        else
        {
            _customMessage = message;
        }
        return this;
    }

    public IRuleBuilder<T, TProperty> When(Func<T, bool> condition)
    {
        rule.Condition = condition;
        return this;
    }

    public IRuleBuilder<T, TProperty> And()
    {
        _orNext = false;
        return this;
    }

    public IRuleBuilder<T, TProperty> Or()
    {
        _orNext = true;
        return this;
    }

    private void AddRuleInternal(Func<T, TProperty, CancellationToken, Task<string?>> newRule)
    {
        if (!_orNext || rule.Rules.Count == 0)
        {
            rule.Rules.Add(newRule);
            _orNext = false;
            return;
        }

        var lastIndex = rule.Rules.Count - 1;
        var previousRule = rule.Rules[lastIndex];

        rule.Rules[lastIndex] = async (instance, value, ct) =>
        {
            var previousResult = await previousRule(instance, value, ct);
            if (previousResult == null) return null;
            return await newRule(instance, value, ct);
        };

        _orNext = false;
    }
}