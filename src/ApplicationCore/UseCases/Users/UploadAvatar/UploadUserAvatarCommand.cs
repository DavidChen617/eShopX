namespace ApplicationCore.UseCases.Users.UploadAvatar;

public record UploadUserAvatarCommand(
    Guid UserId,
    ImageUploadRequest Image) : IRequest<UploadUserAvatarResponse>;
