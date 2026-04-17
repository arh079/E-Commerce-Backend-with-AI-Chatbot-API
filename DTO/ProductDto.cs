namespace ChatAPI.DTO
{
    public class ProductDto
    {
        public Guid ProductId { get; set; }

        public string ProductName { get; set; }

        public float Price { get; set; }

        public string ImageUrl { get; set; }
        public string? Description { get; set; }
    }
}
