using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using eShopX.Application.Exceptions;
using AppImageUploadResult = eShopX.Application.Interfaces.ImageUploadResult;

namespace Infrastructure.Image;

public class ImageStorage(Cloudinary cloudinary) : IImageStorage
{
    public async Task<AppImageUploadResult> UploadAsync(
        ImageUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        var param = new ImageUploadParams
        {
            File = new FileDescription(request.FileName, request.Content)
        };
        var result = await cloudinary.UploadAsync(param, cancellationToken);

        if (result.Error != null)
            throw new ExternalServiceException("Cloudinary", result.Error.Message);

        return new AppImageUploadResult(
            request.FileName,
            result.SecureUrl?.ToString() ?? result.Url?.ToString() ?? string.Empty,
            result.PublicId,
            result.Format ?? string.Empty,
            result.Width,
            result.Height,
            result.Bytes);
    }

    public async Task DeleteAsync(string publicId, CancellationToken cancellationToken = default)
    {
        var result = await cloudinary.DestroyAsync(new DeletionParams(publicId));

        if (result.Error != null)
            throw new ExternalServiceException("Cloudinary", result.Error.Message);
    }
}
