using AutoMapper;
using LudereAI.Shared.DTOs;
using LudereAI.WPF.Models;

namespace LudereAI.WPF.Mappers;

public class ModelsMappingProfile : Profile
{
    public ModelsMappingProfile()
    {
        CreateMap<ConversationDTO, Conversation>();
        CreateMap<MessageDTO, Message>();
        
        CreateMap<Conversation, ConversationDTO>();
        CreateMap<Message, MessageDTO>();
    }
}