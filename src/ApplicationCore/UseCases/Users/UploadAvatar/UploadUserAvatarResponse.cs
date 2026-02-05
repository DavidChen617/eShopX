namespace ApplicationCore.UseCases.Users.UploadAvatar;

public record UploadUserAvatarResponse(
    string Url,
    string PublicId,
    string Format,
    int Width,
    int Height,
    long Bytes);