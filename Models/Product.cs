using System.ComponentModel.DataAnnotations;

namespace ChatAPI.Models
{
    public class Product
    {
        [Key]
        public Guid ProductId { get; set; } = Guid.NewGuid();

        [Required]
        public string ProductName { get; set; }

        [Required]
        public float Price { get; set; }

        [Required]
        public string ImageUrl { get; set; }
        public string? Description { get; set; }

        public ICollection<UserWishlist> UserWishlists { get; set; }
    }
}
