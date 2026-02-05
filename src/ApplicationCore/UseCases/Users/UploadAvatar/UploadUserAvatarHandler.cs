namespace ApplicationCore.UseCases.Users.UploadAvatar;

public class UploadUserAvatarHandler(
    IRepository<User> userRepository,
    IImageStorage imageStorage,
    IMapper mapper)
    : IRequestHandler<UploadUserAvatarCommand, UploadUserAvatarResponse>
{
    public async Task<UploadUserAvatarResponse> Handle(
        UploadUserAvatarCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), command.UserId);

        var uploadResult = await imageStorage.UploadAsync(command.Image, cancellationToken);

        var oldPublicId = user.AvatarPublicId;

        user.AvatarUrl = uploadResult.Url;
        user.AvatarPublicId = uploadResult.PublicId;
        user.AvatarFormat = uploadResult.Format;
        user.AvatarWidth = uploadResult.Width;
        user.AvatarHeight = uploadResult.Height;
        user.AvatarBytes = uploadResult.Bytes;

        await userRepository.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(oldPublicId))
            await imageStorage.DeleteAsync(oldPublicId, cancellationToken);

        return mapper.Map<User, UploadUserAvatarResponse>(user);
    }
}