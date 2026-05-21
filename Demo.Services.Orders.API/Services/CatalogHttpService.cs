namespace Demo.Services.Orders.API.Services
{
    public class CatalogHttpService
    {
        private readonly HttpClient _httpClient;

        public CatalogHttpService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal?> GetProductPriceAsync(int productId)
        {
            // Calls: https://localhost:7088/api/Products/{id}/price
            var response = await _httpClient.GetAsync($"/api/Products/{productId}/price");

            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            return decimal.Parse(content); // Parses the decimal value response
        }
    }
}
