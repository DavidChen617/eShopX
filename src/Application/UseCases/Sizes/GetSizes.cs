using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using eShopX.Application.Interfaces.Repositories;

namespace eShopX.Application.UseCases.Sizes;

public record GetSizesQuery : IRequest<Result<IReadOnlyList<SizeResponse>>>;

public record SizeResponse(Guid Id, string Name, string Type, DateTime CreatedAt);

public class GetSizesHandler(
    ISizeRepository sizeRepository) : IRequestHandler<GetSizesQuery, Result<IReadOnlyList<SizeResponse>>>
{
    public async Task<Result<IReadOnlyList<SizeResponse>>> Handle(
        GetSizesQuery query,
        CancellationToken cancellationToken = default)
    {
        var sizes = await sizeRepository.GetAllAsync(cancellationToken);
        var response = sizes.Select(s => new SizeResponse(s.Id, s.Name, s.Type.ToString(), s.CreatedAt)).ToList();
        return Result<IReadOnlyList<SizeResponse>>.Ok(response);
    }
}
