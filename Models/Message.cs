using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatAPI.Models
{
    public class Message
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey(nameof(Conversation))]
        public Guid ConversationId { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        [MaxLength(20)]
        public string Role { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        public Conversation Conversation { get; set; }
    }
}
