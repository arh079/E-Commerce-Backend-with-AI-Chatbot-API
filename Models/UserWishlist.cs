namespace ChatAPI.Models
{
    public class UserWishlist
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public Guid ProductId { get; set; }
        public Product Product { get; set; }

        public DateTime AddedAt { get; set; }
    }
}
