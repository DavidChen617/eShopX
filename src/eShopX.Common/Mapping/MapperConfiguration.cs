using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace eShopX.Common.Mapping;

public sealed class MapperConfiguration
{
    private readonly Dictionary<TypePair, IMappingPlan> _plans = new();
    private readonly Dictionary<TypePair, MappingDefinition> _definitions = new();
    private readonly ConcurrentDictionary<TypePair, object> _typedPlanCache = new();

    public MapperConfiguration(IEnumerable<Profile> profiles)
    {
        foreach (var profile in profiles)
        {
            foreach (var map in profile.GetMaps())
            {
                _plans[map.TypePair] = BuildPlan(map);
                _definitions[map.TypePair] = map;
            }
        }
    }

    internal bool TryGetPlan<TSource, TDestination>(out MappingPlan<TSource, TDestination>? plan)
    {
        var key = new TypePair(typeof(TSource), typeof(TDestination));

        if (_typedPlanCache.TryGetValue(key, out var cached))
        {
            plan = (MappingPlan<TSource, TDestination>)cached;
            return true;
        }

        if (_plans.TryGetValue(key, out var basePlan))
        {
            plan = (MappingPlan<TSource, TDestination>)basePlan;
            _typedPlanCache[key] = plan;
            return true;
        }

        plan = null;
        return false;
    }

    internal bool TryGetPlan(Type source, Type destination, out IMappingPlan? plan)
        => _plans.TryGetValue(new TypePair(source, destination), out plan);

    private static IMappingPlan BuildPlan(MappingDefinition definition)
    {
        var sourceType = definition.TypePair.Source;
        var destinationType = definition.TypePair.Destination;

        var sourceParam = Expression.Parameter(sourceType, "source");
        var destinationVar = Expression.Variable(destinationType, "destination");

        Expression createDestination;
        if (definition.ConstructUsing is not null)
        {
            var construct = Expression.Constant(definition.ConstructUsing);
            var invoke = Expression.Invoke(construct, Expression.Convert(sourceParam, typeof(object)));
            createDestination = Expression.Convert(invoke, destinationType);
        }
        else
        {
            var ctor = destinationType.GetConstructor(Type.EmptyTypes);
            if (ctor is null)
                throw new InvalidOperationException(
                    $"No parameterless constructor for {destinationType}. Use ConstructUsing.");

            createDestination = Expression.New(ctor);
        }

        var destinationProperties = destinationType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name);

        var sourceProperties = sourceType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToDictionary(p => p.Name);

        var assignments = new List<Expression>
        {
            Expression.Assign(destinationVar, createDestination)
        };

        foreach (var custom in definition.CustomMemberMaps)
        {
            if (!destinationProperties.TryGetValue(custom.DestinationName, out var destinationProperty)) continue;
            if (custom.Resolver is not null)
            {
                var resolver = Expression.Constant(custom.Resolver);
                var valueExpr = Expression.Invoke(resolver, Expression.Convert(sourceParam, typeof(object)));
                var assignValue = Expression.Convert(valueExpr, destinationProperty.PropertyType);
                assignments.Add(Expression.Assign(Expression.Property(destinationVar, destinationProperty), assignValue));
                continue;
            }

            if (custom.SourceName is null) continue;
            if (!sourceProperties.TryGetValue(custom.SourceName, out var sourceProperty)) continue;
            if (!CanAssign(sourceProperty.PropertyType, destinationProperty.PropertyType)) continue;

            var sourceExpr = Expression.Property(sourceParam, sourceProperty);
            var convertedExpr = sourceProperty.PropertyType != destinationProperty.PropertyType
                ? Expression.Convert(sourceExpr, destinationProperty.PropertyType)
                : (Expression)sourceExpr;

            assignments.Add(Expression.Assign(
                Expression.Property(destinationVar, destinationProperty),
                convertedExpr));
        }

        foreach (var destinationProperty in destinationProperties.Values)
        {
            if (definition.CustomMemberMaps.Any(m => m.DestinationName == destinationProperty.Name)) continue;
            if (!sourceProperties.TryGetValue(destinationProperty.Name, out var sourceProperty)) continue;
            if (!CanAssign(sourceProperty.PropertyType, destinationProperty.PropertyType)) continue;

            var sourceExpr = Expression.Property(sourceParam, sourceProperty);
            var convertedExpr = sourceProperty.PropertyType != destinationProperty.PropertyType
                ? Expression.Convert(sourceExpr, destinationProperty.PropertyType)
                : (Expression)sourceExpr;

            assignments.Add(Expression.Assign(
                Expression.Property(destinationVar, destinationProperty),
                convertedExpr));
        }

        assignments.Add(destinationVar);

        var body = Expression.Block(new[] { destinationVar }, assignments);

        var funcType = typeof(Func<,>).MakeGenericType(sourceType, destinationType);
        var lambda = Expression.Lambda(funcType, body, sourceParam);
        var compiled = lambda.Compile();

        var planType = typeof(MappingPlan<,>).MakeGenericType(sourceType, destinationType);
        return (IMappingPlan)Activator.CreateInstance(planType, compiled)!;
    }

    private static bool CanAssign(Type sourceType, Type destinationType)
    {
        if (destinationType.IsAssignableFrom(sourceType))
            return true;

        // Handle Nullable<T>: T can be assigned to T?
        var nullableUnderlying = Nullable.GetUnderlyingType(destinationType);
        if (nullableUnderlying is not null && nullableUnderlying.IsAssignableFrom(sourceType))
            return true;

        return false;
    }

    public void AssertConfigurationIsValid()
    {
        var errors = new List<string>();

        foreach (var (typePair, definition) in _definitions)
        {
            var unmapped = GetUnmappedDestinationProperties(definition);
            if (unmapped.Count > 0)
            {
                errors.Add(
                    $"Unmapped properties for {typePair.Source.Name} -> {typePair.Destination.Name}: " +
                    string.Join(", ", unmapped));
            }
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                "Mapper configuration is invalid:" + Environment.NewLine +
                string.Join(Environment.NewLine, errors));
        }
    }

    private static List<string> GetUnmappedDestinationProperties(MappingDefinition definition)
    {
        var sourceType = definition.TypePair.Source;
        var destinationType = definition.TypePair.Destination;

        var sourceProperties = sourceType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .Select(p => p.Name)
            .ToHashSet();

        var customMappedProperties = definition.CustomMemberMaps
            .Select(m => m.DestinationName)
            .ToHashSet();

        var destinationProperties = destinationType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite);

        return destinationProperties
            .Where(p => !sourceProperties.Contains(p.Name) && !customMappedProperties.Contains(p.Name))
            .Select(p => p.Name)
            .ToList();
    }
}