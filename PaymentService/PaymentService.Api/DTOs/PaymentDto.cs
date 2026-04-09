namespace PaymentService.Api.DTOs;

public class PaymentDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Completed";
    public DateTime CreatedAt { get; set; }
}