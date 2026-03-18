using CoreMesh.Dispatching.Abstractions;
using CoreMesh.Result;
using CoreMesh.Result.Extensions;

namespace Application.UseCases.Users;

public record UpdateUserAvatarCommand(Guid UserId, string FileName, Stream Content)
    : IRequest<Result<UpdateUserAvatarResponse>>;

public record UpdateUserAvatarResponse(string Url);

public class UpdateUserAvatarHandler(
    IUserRepository userRepository,
    IImageStorage imageStorage,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateUserAvatarCommand, Result<UpdateUserAvatarResponse>>
{
    public async Task<Result<UpdateUserAvatarResponse>> Handle(
        UpdateUserAvatarCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
            return Result<UpdateUserAvatarResponse>.NotFound(new Error("user_not_found", "User not found."));

        var upload = await imageStorage.UploadAsync(
            new ImageUploadRequest(command.FileName, command.Content),
            cancellationToken);

        if (user.Avatar is not null)
            await imageStorage.DeleteAsync(user.Avatar.PublicId, cancellationToken);

        user.UpdateAvatar(upload.Url, upload.PublicId);
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UpdateUserAvatarResponse>.Ok(new UpdateUserAvatarResponse(upload.Url));
    }
}
