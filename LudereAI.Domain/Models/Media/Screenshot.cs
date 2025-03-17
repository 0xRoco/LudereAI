using System.ComponentModel.DataAnnotations;
using LudereAI.Domain.Models.Chat;

namespace LudereAI.Domain.Models.Media;

public class Screenshot : BaseEntity
{
    
    [Required]
    public string Base64 { get; set; }
    public Message Message { get; set; }
    
    public string MessageId { get; set; }

}