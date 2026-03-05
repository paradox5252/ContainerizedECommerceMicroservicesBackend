// public interface IOrderClient
// {
//     Task<bool> OrderExistsAsync(int orderId);
// }
using PaymentService.Api.DTOs;

public interface IOrderClient
{
    Task<OrderDto?> GetOrderAsync(int orderId);
}