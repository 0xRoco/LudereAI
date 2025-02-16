using OpenAI.Audio;
using OpenAI.Chat;

namespace LudereAI.Application.Interfaces;

public interface IChatClientFactory
{
    ChatClient CreateOpenAIClient();
    ChatClient CreateGeminiClient();
    AudioClient CreateAudioClient();
}