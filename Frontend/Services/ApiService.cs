using System.Net.Http.Json;
using Frontend.Models;

namespace Frontend.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;

        public ApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<ProductDto>> GetProductsAsync()
        {
            return await _http.GetFromJsonAsync<List<ProductDto>>("/gateway/products");
        }

        public async Task<ProductDto?> AddProductAsync(ProductDto product)
        {
            var response = await _http.PostAsJsonAsync("/gateway/products", product);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ProductDto>();
        }

        public async Task<List<CustomerDto>> GetCustomersAsync()
        {
            return await _http.GetFromJsonAsync<List<CustomerDto>>("/gateway/customers")
                   ?? new List<CustomerDto>();
        }

        public async Task<CustomerDto?> AddCustomerAsync(CustomerDto customer)
        {
            var response = await _http.PostAsJsonAsync("/gateway/customers", customer);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<CustomerDto>();
        }

        public async Task<List<OrderDto>> GetOrdersAsync()
        {
            return await _http.GetFromJsonAsync<List<OrderDto>>("/gateway/orders")
                   ?? new List<OrderDto>();
        }

        public async Task<OrderDto?> CreateOrderAsync(CreateOrderDto order)
        {
            var response = await _http.PostAsJsonAsync("/gateway/orders", order);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<OrderDto>();
        }

        public async Task<OrderDto?> CancelOrderAsync(int orderId)
        {
            var response = await _http.DeleteAsync($"/gateway/orders/{orderId}/cancel");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<OrderDto>();
        }
    }
}
