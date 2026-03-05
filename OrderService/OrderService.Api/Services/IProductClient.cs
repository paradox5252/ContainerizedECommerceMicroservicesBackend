// public interface IProductClient
// {
//     Task<bool> ProductExistsAsync(int productId);
// }
using OrderService.Api.DTOs;
public interface IProductClient
{
    Task<ProductDto?> GetProductAsync(int productId);
}