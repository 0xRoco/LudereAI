using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models;
using LudereAI.Domain.Models.Chat;
using Minio;
using Minio.DataModel.Args;

namespace LudereAI.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName;
    private const string BaseUrl = "https://cdn.mdnite.dev";
    private const string Directory = "ugc";
    private const string FileExtension = ".jpg";
    private const string ContentType = "image/jpeg";

    public FileStorageService(IMinioClientFactory minioClientFactory)
    {
        _minioClient = minioClientFactory.CreateClient();
        
        _bucketName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
            ? "game-assistant-local"
            : "game-assistant";
        
    }
    
    public async Task<StoredFile> UploadFileAsync(string base64Content, Conversation conversation)
    {
        var screenshotId = Guid.NewGuid().ToString("N");
        var fileName = $"{screenshotId}{FileExtension}";
        var path = $"{Directory}/{conversation.AccountId}/{conversation.Id}/{fileName}";
        
        var screenshotBytes = Convert.FromBase64String(base64Content);
        var stream = new MemoryStream(screenshotBytes);
        
        await _minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(path)
            .WithContentType(ContentType)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length));
        
        var url = $"{BaseUrl}/{_bucketName}/{path}";
        
        return new StoredFile
        {
            Id = screenshotId,
            Url = url
        };
    }
}