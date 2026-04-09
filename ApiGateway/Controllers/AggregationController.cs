using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ApiGateway.Controllers;

[ApiController]
[Route("aggregate")]
public class AggregationController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AggregationController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpGet("order-details/{orderId:int}")]
    public async Task<IActionResult> GetOrderDetails(int orderId)
    {
        var orderServiceUrl = _configuration["ServiceUrls:OrderService"];
        var customerServiceUrl = _configuration["ServiceUrls:CustomerService"];
        var productServiceUrl = _configuration["ServiceUrls:ProductService"];

        if (string.IsNullOrWhiteSpace(orderServiceUrl) ||
            string.IsNullOrWhiteSpace(customerServiceUrl) ||
            string.IsNullOrWhiteSpace(productServiceUrl))
        {
            return Problem("Gateway service URLs are not configured.", statusCode: 500);
        }

        var client = _httpClientFactory.CreateClient();

        var orderResponse = await client.GetAsync($"{orderServiceUrl}/api/orders/{orderId}");
        if (!orderResponse.IsSuccessStatusCode)
        {
            return StatusCode((int)orderResponse.StatusCode,
                await orderResponse.Content.ReadAsStringAsync());
        }

        var orderJson = await orderResponse.Content.ReadAsStringAsync();

        using var orderDoc = JsonDocument.Parse(orderJson);
        if (!orderDoc.RootElement.TryGetProperty("customerId", out var customerIdElement) ||
            !orderDoc.RootElement.TryGetProperty("productId", out var productIdElement))
        {
            return Problem("Order payload is missing customerId/productId.", statusCode: 500);
        }

        var customerId = customerIdElement.GetInt32();
        var productId = productIdElement.GetInt32();

        var customerResponse = await client.GetAsync($"{customerServiceUrl}/api/customers/{customerId}");
        if (!customerResponse.IsSuccessStatusCode)
        {
            return StatusCode((int)customerResponse.StatusCode,
                await customerResponse.Content.ReadAsStringAsync());
        }

        var productResponse = await client.GetAsync($"{productServiceUrl}/api/products/{productId}");
        if (!productResponse.IsSuccessStatusCode)
        {
            return StatusCode((int)productResponse.StatusCode,
                await productResponse.Content.ReadAsStringAsync());
        }

        var customerJson = await customerResponse.Content.ReadAsStringAsync();
        var productJson = await productResponse.Content.ReadAsStringAsync();

        var merged = $"{{\"order\":{orderJson},\"customer\":{customerJson},\"product\":{productJson}}}";
        return Content(merged, "application/json");
    }
}
