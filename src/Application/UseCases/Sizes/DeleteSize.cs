using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;

namespace Application.UseCases.Sizes;

public record DeleteSizeCommand(Guid SizeId) : IRequest<Result>;

public class DeleteSizeHandler(
    ISizeRepository sizeRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteSizeCommand, Result>
{
    public async Task<Result> Handle(
        DeleteSizeCommand command,
        CancellationToken cancellationToken = default)
    {
        var size = await sizeRepository.GetByIdAsync(command.SizeId, cancellationToken);
        if (size is null)
            return Result.NotFound(new Error("size_not_found", "Size not found."));

        sizeRepository.Delete(size);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.NoContent();
    }
}
