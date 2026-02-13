using ApplicationCore.UseCases.Products.GetProducts;
using eShopX.Common.Proxy;

namespace ApplicationCore.Interfaces;

[UseDispatchProxy(typeof(LoggingProxy<>))]
public interface IProductSearchService: IInterceptable
{
    Task<GetProductResponse> SearchAsync(GetProductsQuery query, CancellationToken cancellationToken = default);
}
