using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace eShopX.Common.Endpoints;

public interface IEndpoint
{
    void AddRoute(IEndpointRouteBuilder app);
}

public interface IEndpoint<TRequest> : IEndpoint
{
    Task<IResult> HandleAsync(TRequest request);
}

public interface IEndpoint<TRequest, TResponse> : IEndpoint
{
    Task<TResponse> HandleAsync(TRequest request);
}

public interface IGroupEndpoint
{
    string GroupPrefix { get; }
    void Configure(RouteGroupBuilder group);
}

public interface IGroupedEndpoint
{
    Type GroupType { get; }
    void AddRoute(RouteGroupBuilder group);
}

public interface IGroupedEndpoint<TGroup> : IGroupedEndpoint where TGroup : IGroupEndpoint
{
    Type IGroupedEndpoint.GroupType => typeof(TGroup);
}

public interface IGroupedEndpoint<TGroup, TRequest> : IGroupedEndpoint where TGroup : IGroupEndpoint
{
    Type IGroupedEndpoint.GroupType => typeof(TGroup);
    Task<IResult> HandleAsync(TRequest request);
}

public interface IGroupedEndpoint<TGroup, TRequest, TResponse> : IGroupedEndpoint where TGroup : IGroupEndpoint
{
    Type IGroupedEndpoint.GroupType => typeof(TGroup);
    Task<TResponse> HandleAsync(TRequest request);
}