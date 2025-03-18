using AutoMapper;
using LudereAI.Shared.DTOs;
using LudereAI.Domain.Models;
using LudereAI.Domain.Models.Account;
using LudereAI.Domain.Models.Chat;

namespace LudereAI.Domain.Mappers;

public class DomainMappingProfile : Profile
{
    public DomainMappingProfile()
    {
        CreateMap<Account, AccountDTO>();
        CreateMap<UpdateAccountDTO, Account>();
        CreateMap<SignUpDTO, Account>();
        
        CreateMap<Conversation, ConversationDTO>();
        CreateMap<ConversationDTO, Conversation>();
        
        CreateMap<Message, MessageDTO>();
        CreateMap<MessageDTO, Message>();
        
        CreateMap<UserSubscription, UserSubscriptionDTO>();
        CreateMap<UserSubscriptionDTO, UserSubscription>();
    }
}