// using System.Net;

// public class OrderClient : IOrderClient
// {
//     private readonly HttpClient _httpClient;

//     public OrderClient(HttpClient httpClient)
//     {
//         _httpClient = httpClient;
//     }

//     public async Task<bool> OrderExistsAsync(int orderId)
//     {
//         var response = await _httpClient.GetAsync(
//             $"api/orders/{orderId}");

//         return response.StatusCode == HttpStatusCode.OK;
//     }
// }


using PaymentService.Api.DTOs;

public class OrderClient : IOrderClient
{
    private readonly HttpClient _httpClient;

    public OrderClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<OrderDto?> GetOrderAsync(int orderId)
    {
        var response = await _httpClient.GetAsync(
            $"api/orders/{orderId}");

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<OrderDto>();
    }
}