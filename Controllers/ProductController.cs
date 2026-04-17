using ChatAPI.Data;
using ChatAPI.DTO;
using ChatAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext context;

        public ProductController(AppDbContext _context)
        {
            context = _context;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(CreateProductDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Product Data Is Required..");
            }
            var exists = await context.Products
                .AnyAsync(x => x.ProductName == dto.ProductName && x.ImageUrl == dto.ImageUrl);

            if (exists)
                return BadRequest("Product already in Database..");

            var product = new Product
            {
                ProductName = dto.ProductName,
                Price = dto.Price,
                ImageUrl = dto.ImageUrl,
                Description = dto.Description
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            return Ok(new ProductDto
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                Description= product.Description
            });
        }
        [HttpGet("GetByName/{Name}")]
        public IActionResult Get(string Name) 
        {
                var product = context.Products.SingleOrDefault(x=>x.ProductName == Name);

                if (product == null)
                    return NotFound();

                return Ok(product);
        }

        [HttpGet("GetProducts")]
        public async Task<IActionResult> GetAll()
        {
            var products = await context.Products
                .Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    Description= p.Description
                })
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            return Ok(product);
        }
        [HttpDelete("DeleteById/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var product = await context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            context.Products.Remove(product);
            await context.SaveChangesAsync();

            return Ok("Removed");
        }
    }
}
