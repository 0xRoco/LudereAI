using System.ComponentModel.DataAnnotations;
using LudereAI.Domain.Models.Chat;

namespace LudereAI.Domain.Models.Media;

public class Screenshot : BaseEntity
{
    
    [Required]
    public string Url { get; set; }
    public Message Message { get; set; }
    
    // Foreign Key
    public string MessageId { get; set; }

}