using System.ComponentModel.DataAnnotations;
using LudereAI.Core.Entities.Chat;

namespace LudereAI.Core.Entities.Media;

public class Screenshot : BaseEntity
{
    
    [Required]
    public string Base64 { get; set; }
    public Message Message { get; set; }
    
    public string MessageId { get; set; }

}