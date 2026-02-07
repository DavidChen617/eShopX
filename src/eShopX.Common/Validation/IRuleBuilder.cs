namespace eShopX.Common.Validation;

public interface IRuleBuilder<T, TProperty>
{
    IRuleBuilder<T, TProperty> AddRule(Func<TProperty, bool> predicate, string defaultMessage);
    IRuleBuilder<T, TProperty> AddRule(Func<T, TProperty, bool> predicate, string defaultMessage);

    IRuleBuilder<T, TProperty> AddAsyncRule(Func<TProperty, CancellationToken, Task<bool>> predicate, string defaultMessage);
    IRuleBuilder<T, TProperty> NotNull();
    IRuleBuilder<T, TProperty> NotEmpty();
    IRuleBuilder<T, TProperty> MaxLength(int max);
    IRuleBuilder<T, TProperty> MinLength(int min);
    IRuleBuilder<T, TProperty> Email();
    IRuleBuilder<T, TProperty> GreaterThan(TProperty value);
    IRuleBuilder<T, TProperty> LessThan(TProperty value);
    IRuleBuilder<T, TProperty> Must(Func<TProperty, bool> predicate);
    IRuleBuilder<T, TProperty> MustAsync(Func<TProperty, CancellationToken, Task<bool>> predicate);
    IRuleBuilder<T, TProperty> Matches(string pattern);
    IRuleBuilder<T, TProperty> InclusiveBetween(TProperty from, TProperty to);
    IRuleBuilder<T, TProperty> IsInEnum();
    IRuleBuilder<T, TProperty> Equal(TProperty value);
    IRuleBuilder<T, TProperty> Equal(Func<T, TProperty> selector);
    IRuleBuilder<T, TProperty> NotEqual(TProperty value);
    IRuleBuilder<T, TProperty> WithMessage(string message);
    IRuleBuilder<T, TProperty> When(Func<T, bool> condition);
    IRuleBuilder<T, TProperty> And();
    IRuleBuilder<T, TProperty> Or();
}
