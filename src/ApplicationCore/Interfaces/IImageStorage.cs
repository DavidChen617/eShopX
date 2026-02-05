namespace ApplicationCore.Interfaces;

public interface IImageStorage
{
    Task<ImageUploadResult> UploadAsync(ImageUploadRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(string publicId, CancellationToken cancellationToken = default);
}

public record ImageUploadRequest(
    Stream Content,
    string FileName,
    string ContentType,
    long Length);

public record ImageUploadResult(
    string Url,
    string PublicId,
    string Format,
    int Width,
    int Height,
    long Bytes);