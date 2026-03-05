// using System.Net;

// public class ProductClient : IProductClient
// {
//     private readonly HttpClient _httpClient;

//     public ProductClient(HttpClient httpClient)
//     {
//         _httpClient = httpClient;
//     }

//     public async Task<bool> ProductExistsAsync(int productId)
//     {
//         var response = await _httpClient.GetAsync(
//             $"api/products/{productId}");

//         return response.StatusCode == HttpStatusCode.OK;
//     }
// }

using OrderService.Api.DTOs;
public class ProductClient : IProductClient
{
    private readonly HttpClient _httpClient;

    public ProductClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductDto?> GetProductAsync(int productId)
    {
        var response = await _httpClient.GetAsync(
            $"api/products/{productId}");

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<ProductDto>();
    }
}