using OpenAI.Audio;
using OpenAI.Chat;

namespace LudereAI.Application.Interfaces;

public interface IChatClientFactory
{
    ChatClient CreateChatClient();
    
    Task<byte[]> GenerateAudio(string text);
}