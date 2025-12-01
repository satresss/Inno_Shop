namespace ProductService.Application.Exceptions
{
    public class ProductNotFoundException : Exception
    {
        public ProductNotFoundException(int productId) 
            : base($"Product with ID {productId} was not found.")
        {
        }

        public ProductNotFoundException(string message) : base(message)
        {
        }
    }
}

