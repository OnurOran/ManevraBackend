using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace MyApp.Api.Infrastructure.Storage;

public class StorageOptions
{
    public const string SectionName = "Storage";
    public string ServiceUrl { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
}

public class HetznerFileService : IFileService
{
    private readonly IAmazonS3 _s3Client;
    private readonly StorageOptions _options;

    public HetznerFileService(IAmazonS3 s3Client, IOptions<StorageOptions> options)
    {
        _s3Client = s3Client;
        _options = options.Value;
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct)
    {
        var key = $"{Guid.NewGuid():N}/{fileName}";
        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = fileStream,
            ContentType = contentType
        };
        await _s3Client.PutObjectAsync(request, ct);
        return key;
    }

    public async Task<Stream> DownloadAsync(string fileKey, CancellationToken ct)
    {
        var response = await _s3Client.GetObjectAsync(_options.BucketName, fileKey, ct);
        return response.ResponseStream;
    }

    public async Task DeleteAsync(string fileKey, CancellationToken ct)
    {
        await _s3Client.DeleteObjectAsync(_options.BucketName, fileKey, ct);
    }

    public Task<string> GetPublicUrlAsync(string fileKey, CancellationToken ct)
    {
        var url = $"{_options.ServiceUrl.TrimEnd('/')}/{_options.BucketName}/{fileKey}";
        return Task.FromResult(url);
    }
}
