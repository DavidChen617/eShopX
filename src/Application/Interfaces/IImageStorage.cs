namespace Application.Interfaces;

public interface IImageStorage
{
    Task<ImageUploadResult> UploadAsync(ImageUploadRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(string publicId, CancellationToken cancellationToken = default);
}

public record ImageUploadRequest(string FileName, Stream Content);

public record ImageUploadResult(
    string FileName,
    string Url,
    string PublicId,
    string Format,
    int Width,
    int Height,
    long Bytes);
