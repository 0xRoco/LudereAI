using LudereAI.Domain.Models;
using LudereAI.Domain.Models.Account;
using LudereAI.Shared.DTOs;

namespace LudereAI.Application.Interfaces;

public interface IAccountFactory
{
    Account Create(SignUpDTO dto);
    Account Create(GuestDTO dto);
}