namespace ApplicationCore.UseCases.Users.DeleteAvatar;

public class DeleteUserAvatarHandler(
    IRepository<User> userRepository,
    IImageStorage imageStorage)
    : IRequestHandler<DeleteUserAvatarCommand>
{
    public async Task Handle(DeleteUserAvatarCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), command.UserId);

        if (!string.IsNullOrWhiteSpace(user.AvatarPublicId))
        {
            await imageStorage.DeleteAsync(user.AvatarPublicId, cancellationToken);
        }

        user.AvatarUrl = null;
        user.AvatarPublicId = null;
        user.AvatarFormat = null;
        user.AvatarWidth = null;
        user.AvatarHeight = null;
        user.AvatarBytes = null;

        await userRepository.SaveChangesAsync(cancellationToken);
    }
}
