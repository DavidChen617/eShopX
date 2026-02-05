namespace ApplicationCore.UseCases.Users.DeleteAvatar;

public record DeleteUserAvatarCommand(Guid UserId) : IRequest;