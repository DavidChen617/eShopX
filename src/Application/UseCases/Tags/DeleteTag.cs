using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using eShopX.Application.Interfaces;
using eShopX.Application.Interfaces.Repositories;

namespace eShopX.Application.UseCases.Tags;

public record DeleteTagCommand(Guid TagId) : IRequest<Result>;

public class DeleteTagHandler(
    ITagRepository tagRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteTagCommand, Result>
{
    public async Task<Result> Handle(
        DeleteTagCommand command,
        CancellationToken cancellationToken = default)
    {
        var tag = await tagRepository.GetByIdAsync(command.TagId, cancellationToken);
        if (tag is null)
            return Result.NotFound(new Error("tag_not_found", "Tag not found."));

        tagRepository.Delete(tag);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.NoContent();
    }
}
