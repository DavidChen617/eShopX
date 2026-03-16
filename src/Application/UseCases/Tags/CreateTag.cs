using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using eShopX.Application.Interfaces;
using eShopX.Application.Interfaces.Repositories;
using eShopX.Domain.Aggregates.Tags;

namespace eShopX.Application.UseCases.Tags;

public record CreateTagCommand(string Name, TagType Type) : IRequest<Result<TagResponse>>;

public class CreateTagHandler(
    ITagRepository tagRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateTagCommand, Result<TagResponse>>
{
    public async Task<Result<TagResponse>> Handle(
        CreateTagCommand command,
        CancellationToken cancellationToken = default)
    {
        var tag = Tag.Create(command.Name, command.Type);
        await tagRepository.AddAsync(tag, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<TagResponse>.Created(new TagResponse(tag.Id, tag.Name, tag.Type.ToString(), tag.CreatedAt));
    }
}
