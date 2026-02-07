namespace eShopX.Common.Mapping;

internal interface IMappingPlan
{
    object MapObject(object source);
}

internal sealed class MappingPlan<TSource, TDestination>(Func<TSource, TDestination> map) : IMappingPlan
{
    public TDestination Map(TSource source) => map(source);

    object IMappingPlan.MapObject(object source) => Map((TSource)source)!;
}
