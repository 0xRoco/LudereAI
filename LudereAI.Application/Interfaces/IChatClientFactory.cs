using OpenAI.Chat;

namespace LudereAI.Application.Interfaces;

public interface IChatClientFactory
{
    ChatClient CreateChatClient();
}