namespace ProductService.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        private Product() { }

        public Product(string name, string description, decimal price, int createdByUserId)
        {
            Name = name;
            Description = description;
            Price = price;
            CreatedByUserId = createdByUserId;
            IsAvailable = true;
            CreatedAt = DateTime.UtcNow;
        }

        public void Update(string name, string description, decimal price, bool isAvailable)
        {
            Name = name;
            Description = description;
            Price = price;
            IsAvailable = isAvailable;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetAvailability(bool isAvailable)
        {
            IsAvailable = isAvailable;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}

