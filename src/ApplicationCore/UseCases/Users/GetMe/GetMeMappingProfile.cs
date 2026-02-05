namespace ApplicationCore.UseCases.Users.GetMe;

public class GetMeMappingProfile : Profile
{
    public GetMeMappingProfile()
    {
        CreateMap<User, GetMeResponse>()
            .ConstructUsing(src => new GetMeResponse(
                src.Id,
                src.Name,
                src.Email,
                src.Phone,
                src.Address,
                src.CreatedAt,
                src.AvatarUrl,
                src.AvatarPublicId,
                src.AvatarFormat,
                src.AvatarWidth,
                src.AvatarHeight,
                src.AvatarBytes,
                src.IsSeller,
                src.IsAdmin,
                src.SellerStatus,
                src.SellerAppliedAt,
                src.SellerApprovedAt,
                src.SellerRejectionReason));
    }
}