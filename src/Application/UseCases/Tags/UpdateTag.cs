using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;

namespace Application.UseCases.Tags;

public record UpdateTagCommand(Guid TagId, string Name) : IRequest<Result>;

public class UpdateTagHandler(
    ITagRepository tagRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateTagCommand, Result>
{
    public async Task<Result> Handle(
        UpdateTagCommand command,
        CancellationToken cancellationToken = default)
    {
        var tag = await tagRepository.GetByIdAsync(command.TagId, cancellationToken);
        if (tag is null)
            return Result.NotFound(new Error("tag_not_found", "Tag not found."));

        tag.Rename(command.Name);
        tagRepository.Update(tag);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.NoContent();
    }
}
