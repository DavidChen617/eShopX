using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using eShopX.Application.Interfaces.Repositories;

namespace eShopX.Application.UseCases.Tags;

public record GetTagsQuery : IRequest<Result<IReadOnlyList<TagResponse>>>;

public record TagResponse(Guid Id, string Name, string Type, DateTime CreatedAt);

public class GetTagsHandler(
    ITagRepository tagRepository) : IRequestHandler<GetTagsQuery, Result<IReadOnlyList<TagResponse>>>
{
    public async Task<Result<IReadOnlyList<TagResponse>>> Handle(
        GetTagsQuery query,
        CancellationToken cancellationToken = default)
    {
        var tags = await tagRepository.GetAllAsync(cancellationToken);
        var response = tags.Select(t => new TagResponse(t.Id, t.Name, t.Type.ToString(), t.CreatedAt)).ToList();
        return Result<IReadOnlyList<TagResponse>>.Ok(response);
    }
}
