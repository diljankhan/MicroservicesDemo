namespace Demo.Services.Orders.API.Services
{
    public class CustomerHttpService
    {
        private readonly HttpClient _httpClient;

        public CustomerHttpService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> CheckCustomerExistsAsync(int customerId)
        {
            // Calls: https://localhost:7180/api/Customers/{id}/exists
            var response = await _httpClient.GetAsync($"/api/Customers/{customerId}/exists");

            if (!response.IsSuccessStatusCode) return false;

            var content = await response.Content.ReadAsStringAsync();
            return bool.Parse(content); // Parses the true/false text response
        }
    }
}
