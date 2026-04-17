using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatAPI.Models
{
    public class Conversation
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [ForeignKey(nameof(User))]
        public string UserId { get; set; }
        [MaxLength(100)]
        public string? Title { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime? LastUpdatedAt { get; set; }

        public ICollection<Message> Messages { get; set; }
        public ApplicationUser User { get; set; }

    }
}
