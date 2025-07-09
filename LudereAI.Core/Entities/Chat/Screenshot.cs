using System.ComponentModel.DataAnnotations;

namespace LudereAI.Core.Entities.Chat;

public class Screenshot : BaseEntity
{

    [Required] public string Base64 { get; set; } = string.Empty;
    public Message Message { get; set; } = new();
    
    public string MessageId { get; set; } = string.Empty;

}