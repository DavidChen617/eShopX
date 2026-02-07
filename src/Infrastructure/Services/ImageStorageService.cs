using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

using eShopX.Common.Exceptions;
using eShopX.Common.Mapping;

namespace Infrastructure.Services;

public class ImageStorageService(Cloudinary cloudinary, IMapper mapper) : IImageStorage
{
    public async Task<ApplicationCore.Interfaces.ImageUploadResult> UploadAsync(
        ImageUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        var param = new ImageUploadParams
        {
            File = new FileDescription(request.FileName, request.Content),
            // Folder = "users/avatars" // optional
        };
        var cloudinaryResult = await cloudinary.UploadAsync(param, cancellationToken);

        if (cloudinaryResult.Error != null)
            throw new BadRequestException($"Cloudinary upload failed: {cloudinaryResult.Error.Message}");

        return mapper.Map<ApplicationCore.Interfaces.ImageUploadResult>(cloudinaryResult);
    }

    public async Task DeleteAsync(string publicId, CancellationToken cancellationToken = default)
    {
        var result = await cloudinary.DestroyAsync(new DeletionParams(publicId));

        if (result.Error != null)
            throw new BadRequestException($"Cloudinary delete failed: {result.Error.Message}");
    }
}

public class CloudinaryProfile : Profile
{
    public CloudinaryProfile()
    {
        CreateMap<CloudinaryDotNet.Actions.ImageUploadResult, ApplicationCore.Interfaces.ImageUploadResult>()
            .ConstructUsing(src => new ApplicationCore.Interfaces.ImageUploadResult(
                src.SecureUrl?.ToString() ?? src.Url?.ToString() ?? string.Empty,
                src.PublicId,
                src.Format ?? string.Empty,
                src.Width,
                src.Height,
                src.Bytes
            ));
    }
}
