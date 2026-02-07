using System.Linq.Expressions;

namespace eShopX.Common.Mapping;

public sealed class MappingExpression<TSource, TDestination>
{
    private readonly MappingDefinition _definition;
    private readonly Profile _profile;

    internal MappingExpression(Profile profile, MappingDefinition definition)
    {
        _profile = profile;
        _definition = definition;
    }

    public MappingExpression<TSource, TDestination> ForMember<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember,
        Expression<Func<TSource, TMember>> sourceMember)
    {
        var destinationName = GetPropertyName(destinationMember);
        var sourceName = TryGetPropertyName(sourceMember);
        var compiled = sourceMember.Compile();
        _definition.AddCustomMember(destinationName, src => compiled((TSource)src), sourceName);
        return this;
    }

    public MappingExpression<TSource, TDestination> ConstructUsing(
        Func<TSource, TDestination> factory)
    {
        _definition.SetConstructUsing(src => factory((TSource)src));
        return this;
    }

    public MappingExpression<TSource, TDestination> ReverseMap()
    {
        var reverseDefinition = _profile.AddReverse(typeof(TDestination), typeof(TSource));
        foreach (var map in _definition.CustomMemberMaps)
        {
            if (map.SourceName is null) continue;
            reverseDefinition.AddCustomMember(map.SourceName, map.DestinationName);
        }
        return this;
    }

    private static string GetPropertyName<TMember>(Expression<Func<TDestination, TMember>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
            return memberExpression.Member.Name;
        if (expression.Body is UnaryExpression { Operand: MemberExpression operand })
            return operand.Member.Name;

        throw new ArgumentException("Invalid expression", nameof(expression));
    }

    private static string? TryGetPropertyName<TMember>(Expression<Func<TSource, TMember>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
            return memberExpression.Member.Name;
        if (expression.Body is UnaryExpression { Operand: MemberExpression operand })
            return operand.Member.Name;
        return null;
    }
}
