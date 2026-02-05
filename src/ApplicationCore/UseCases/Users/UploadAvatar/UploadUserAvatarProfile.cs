namespace ApplicationCore.UseCases.Users.UploadAvatar;

public class UploadUserAvatarProfile : Profile
{
    public UploadUserAvatarProfile()
    {
        CreateMap<User, UploadUserAvatarResponse>()
            .ConstructUsing(src => new UploadUserAvatarResponse(
                src.AvatarUrl ?? string.Empty,
                src.AvatarPublicId ?? string.Empty,
                src.AvatarFormat ?? string.Empty,
                src.AvatarWidth ?? 0,
                src.AvatarHeight ?? 0,
                src.AvatarBytes ?? 0
            ));
    }
}