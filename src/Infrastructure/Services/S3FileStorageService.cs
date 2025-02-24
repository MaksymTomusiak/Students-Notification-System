using Amazon.S3;
using Amazon.S3.Model;
using Application.Common.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class S3FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3FileStorageService(IAmazonS3 s3Client, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _bucketName = configuration["AWS:BucketName"];
    }

    public async Task<string> SaveFileAsync(IFormFile file, string folderName, Guid id, CancellationToken cancellationToken)
    {
        var key = $"{folderName}/{id}";
        
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = memoryStream,
            ContentType = file.ContentType,
            CannedACL = S3CannedACL.PublicRead
        };
        await _s3Client.PutObjectAsync(request, cancellationToken);

        return $"https://{_bucketName}.s3.{_s3Client.Config.RegionEndpoint.SystemName}.amazonaws.com/{key}";
    }
    public async Task<string> DeleteFileAsync(string folderName, Guid id, CancellationToken cancellationToken)
    {
        var key = $"{folderName}/{id}";
        
        await _s3Client.DeleteObjectAsync(_bucketName, key, cancellationToken);

        return $"https://{_bucketName}.s3.{_s3Client.Config.RegionEndpoint.SystemName}.amazonaws.com/{key}";
    }
}