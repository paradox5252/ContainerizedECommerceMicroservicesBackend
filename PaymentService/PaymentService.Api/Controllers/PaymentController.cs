using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentService.Api.Data;
using PaymentService.Api.Models;
// using PaymentService.Api.Services;
using PaymentService.Api.DTOs;

namespace PaymentService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
  private readonly PaymentDbContext _context;
  private readonly IOrderClient _orderClient;

  public PaymentController(
      PaymentDbContext context,
      IOrderClient orderClient)
  {
    _context = context;
    _orderClient = orderClient;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAll()
  {
    var payments = await _context.Payments
      .Select(p => new PaymentDto
      {
        Id = p.Id,
        OrderId = p.OrderId,
        Amount = p.Amount,
        Status = p.Status,
        CreatedAt = p.CreatedAt
      })
      .ToListAsync();

    return Ok(payments);
  }

  [HttpPost]
  public async Task<IActionResult> Create(CreatePaymentDto dto)
  {
    var order =
        await _orderClient.GetOrderAsync(dto.OrderId);

    if (order == null)
      return BadRequest("Order does not exist.");

    if (dto.Amount != order.Total)
      return BadRequest("Payment amount mismatch.");

    var payment = new Payment
    {
      OrderId = dto.OrderId,
      Amount = dto.Amount,
      Status = "Completed"
    };

    await _context.Payments.AddAsync(payment);
    await _context.SaveChangesAsync();

    return Ok(new PaymentDto
    {
      Id = payment.Id,
      OrderId = payment.OrderId,
      Amount = payment.Amount,
      Status = payment.Status,
      CreatedAt = payment.CreatedAt
    });
  }

  // [HttpPost]
  //     public async Task<IActionResult> Create(Payment payment)
  //     {
  //         var order =
  //             await _orderClient.GetOrderAsync(payment.OrderId);

  //         if (order == null)
  //             return BadRequest("Order does not exist.");

  //         // 🔥 验证金额一致
  //         if (payment.Amount != order.Total)
  //             return BadRequest("Payment amount mismatch.");

  //         payment.Status = "Completed";

  //         await _context.Payments.AddAsync(payment);
  //         await _context.SaveChangesAsync();

  //         return Ok(payment);
  //     }
}