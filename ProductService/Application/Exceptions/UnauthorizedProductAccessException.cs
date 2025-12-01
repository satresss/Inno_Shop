namespace ProductService.Application.Exceptions
{
    public class UnauthorizedProductAccessException : Exception
    {
        public UnauthorizedProductAccessException(int productId, int userId) 
            : base($"User {userId} is not authorized to access product {productId}.")
        {
        }

        public UnauthorizedProductAccessException(string message) : base(message)
        {
        }
    }
}

