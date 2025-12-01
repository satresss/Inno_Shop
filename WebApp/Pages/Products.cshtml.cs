using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Services;
using static WebApp.Services.ApiService;

namespace WebApp.Pages
{
    public class ProductsModel : PageModel
    {
        private readonly ApiService _apiService;

        public ProductsModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        public List<ProductDto>? Products { get; set; }
        public string? ErrorMessage { get; set; }
        public bool IsAuthenticated => _apiService.IsAuthenticated();

        // Свойства для фильтров
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? IsAvailable { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public async Task OnGetAsync()
        {
            try
            {
                // Создаем фильтр из параметров запроса
                var filter = new ProductFilterDto
                {
                    SearchTerm = SearchTerm,
                    MinPrice = MinPrice,
                    MaxPrice = MaxPrice,
                    IsAvailable = IsAvailable,
                    PageNumber = PageNumber,
                    PageSize = PageSize
                };

                // Используем поиск с фильтрами
                Products = await _apiService.SearchProductsAsync("ProductsApi", filter);

                if (Products == null)
                {
                    Products = new List<ProductDto>();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
                Products = new List<ProductDto>();
            }
        }
    }
}
