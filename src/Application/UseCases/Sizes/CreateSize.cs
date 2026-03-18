using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;
using Domain.Aggregates.Sizes;

namespace Application.UseCases.Sizes;

public record CreateSizeCommand(string Name, SizeType Type) : IRequest<Result<SizeResponse>>;

public class CreateSizeHandler(
    ISizeRepository sizeRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateSizeCommand, Result<SizeResponse>>
{
    public async Task<Result<SizeResponse>> Handle(
        CreateSizeCommand command,
        CancellationToken cancellationToken = default)
    {
        var size = Size.Create(command.Name, command.Type);
        await sizeRepository.AddAsync(size, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<SizeResponse>.Created(new SizeResponse(size.Id, size.Name, size.Type.ToString(), size.CreatedAt));
    }
}
