using OpenAI.Chat;

namespace LudereAI.Core.Interfaces;

public interface IChatClientFactory
{
    ChatClient CreateChatClient();
}