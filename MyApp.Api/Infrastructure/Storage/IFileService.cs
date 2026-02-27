namespace MyApp.Api.Infrastructure.Storage;

/// <summary>Provides file storage operations against an S3-compatible backend.</summary>
public interface IFileService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct);
    Task<Stream> DownloadAsync(string fileKey, CancellationToken ct);
    Task DeleteAsync(string fileKey, CancellationToken ct);
    Task<string> GetPublicUrlAsync(string fileKey, CancellationToken ct);
}
