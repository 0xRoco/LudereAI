using AutoMapper;
using LudereAI.Core.Entities.Chat;
using LudereAI.WPF.Models;

namespace LudereAI.WPF.Infrastructure;

public class CoreMappingProfile : Profile
{
    public CoreMappingProfile()
    {
        CreateMap<Conversation, ConversationModel>();
        CreateMap<Message, MessageModel>();
    }
}