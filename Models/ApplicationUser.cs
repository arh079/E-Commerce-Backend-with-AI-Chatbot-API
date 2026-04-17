using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ChatAPI.Models
{
    public class ApplicationUser: IdentityUser
    {
      
        public ICollection<Conversation> Conversations { get; set; }
        public ICollection<UserWishlist> Wishlists { get; set; }
    }
}
