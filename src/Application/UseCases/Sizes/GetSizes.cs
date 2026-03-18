using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using Domain.Aggregates.Sizes;

namespace Application.UseCases.Sizes;

public record GetSizesQuery(SizeType? Filter = null) : IRequest<Result<IReadOnlyList<SizeResponse>>>;

public record SizeResponse(Guid Id, string Name, string Type, DateTime CreatedAt);

public class GetSizesHandler(
    ISizeRepository sizeRepository) : IRequestHandler<GetSizesQuery, Result<IReadOnlyList<SizeResponse>>>
{
    public async Task<Result<IReadOnlyList<SizeResponse>>> Handle(
        GetSizesQuery query,
        CancellationToken cancellationToken = default)
    {
        var sizes = await sizeRepository.GetAllAsync(query.Filter, cancellationToken);
        var response = sizes.Select(s => new SizeResponse(s.Id, s.Name, s.Type.ToString(), s.CreatedAt)).ToList();
        return Result<IReadOnlyList<SizeResponse>>.Ok(response);
    }
}
