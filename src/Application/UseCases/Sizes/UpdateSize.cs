using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;

namespace Application.UseCases.Sizes;

public record UpdateSizeCommand(Guid SizeId, string Name) : IRequest<Result>;

public class UpdateSizeHandler(
    ISizeRepository sizeRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateSizeCommand, Result>
{
    public async Task<Result> Handle(
        UpdateSizeCommand command,
        CancellationToken cancellationToken = default)
    {
        var size = await sizeRepository.GetByIdAsync(command.SizeId, cancellationToken);
        if (size is null)
            return Result.NotFound(new Error("size_not_found", "Size not found."));

        size.Rename(command.Name);
        sizeRepository.Update(size);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.NoContent();
    }
}
