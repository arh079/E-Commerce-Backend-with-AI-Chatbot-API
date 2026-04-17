namespace ChatAPI.DTO
{
    public class CreateProductDto
    {
        public string ProductName { get; set; }

        public float Price { get; set; }

        public string ImageUrl { get; set; }
        public string Description { get; set; }
    }
}
