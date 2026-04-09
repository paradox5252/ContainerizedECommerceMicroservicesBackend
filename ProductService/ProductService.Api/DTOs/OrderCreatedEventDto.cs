namespace ProductService.Api.DTOs;

public class OrderCreatedEventDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public int CustomerId { get; set; }
    public decimal Total { get; set; }
}
