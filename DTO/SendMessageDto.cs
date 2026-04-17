using System.ComponentModel.DataAnnotations;

namespace ChatAPI.DTO
{
    public class SendMessageDto
    {
        public Guid ConversationId { get; set; }
        [Required]
        [MinLength(1)]
        public string Message { get; set; }
    }
}
