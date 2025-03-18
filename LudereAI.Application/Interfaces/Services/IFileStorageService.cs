using LudereAI.Domain.Models;
using LudereAI.Domain.Models.Chat;

namespace LudereAI.Application.Interfaces.Services;

public interface IFileStorageService
{
    Task<StoredFile> UploadFileAsync(string base64Content, Conversation conversation);
}