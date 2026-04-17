using ChatAPI.Data;
using ChatAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace ChatAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly AppDbContext context;

        public WishlistController(AppDbContext _context)
        {
            context = _context;
        }

        [HttpPost("AddToWishlist/{productId}")]
        public async Task<IActionResult> Add(Guid productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var exists = await context.Wishlists
                .AnyAsync(x => x.UserId == userId && x.ProductId == productId);

            if (exists)
                return BadRequest("Product already in wishlist");

            context.Wishlists.Add(new UserWishlist
            {
                UserId = userId,
                ProductId = productId,
                AddedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();

            return Ok("Added to wishlist");
        }

        [HttpDelete("Remove/{productId}")]
        public async Task<IActionResult> Remove(Guid productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var item = await context.Wishlists
                .FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId);

            if (item == null)
                return NotFound();

            context.Wishlists.Remove(item);
            await context.SaveChangesAsync();

            return Ok("Removed");
        }

        [HttpGet("GetMyWishlist")]
        public async Task<IActionResult> GetMyWishlist()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var products = await context.Wishlists
                .Where(x => x.UserId == userId)
                .Select(x => new
                {
                    x.Product.ProductId,
                    x.Product.ProductName,
                    x.Product.Price,
                    x.Product.ImageUrl
                })
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("GetByName/{Name}")]
        public IActionResult Get(string Name)
        {
            var product = context.Products.SingleOrDefault(x => x.ProductName == Name);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (product == null)
                return NotFound("NotFound");

           var ProductId = 
                context.Wishlists.Where(x => x.ProductId == product.ProductId  && userId == x.UserId)
                 .Select(x => new { x.ProductId });

            if(ProductId.IsNullOrEmpty())
                return NotFound("NotFound");

            return Ok(ProductId);
        }

    }
}
