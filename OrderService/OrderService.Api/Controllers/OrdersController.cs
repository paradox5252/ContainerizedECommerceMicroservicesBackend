using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Data;
using OrderService.Api.Models;
using OrderService.Api.DTOs;

namespace OrderService.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly OrdersDbContext _context;
    private readonly ICustomerClient _customerClient;
    private readonly IProductClient _productClient;

    public OrdersController(
        OrdersDbContext context,
        ICustomerClient customerClient,
        IProductClient productClient)
    {
        _context = context;
        _customerClient = customerClient;
        _productClient = productClient;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _context.Orders.ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _context.Orders.FindAsync(id);

        if (order == null)
            return NotFound();

        return Ok(order);
    }

    // [HttpPost]
    // public async Task<IActionResult> Create(CreateOrderDto dto)
    // {
    //     var customerExists =
    //         await _customerClient.CustomerExistsAsync(dto.CustomerId);

    //     if (!customerExists)
    //         return BadRequest("Customer does not exist.");

    //     var product =
    //         await _productClient.GetProductAsync(order.ProductId);

    //     if (product == null)
    //         return BadRequest("Product does not exist.");

    //     // 🔥 自动计算 total
    //     order.Total = product.Price * order.Quantity;

    //     await _context.Orders.AddAsync(order);
    //     await _context.SaveChangesAsync();

    //     return Ok(order);
    // }
    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderDto dto)
    {
        var customerExists =
            await _customerClient.CustomerExistsAsync(dto.CustomerId);

        if (!customerExists)
            return BadRequest("Customer does not exist.");

        var product =
            await _productClient.GetProductAsync(dto.ProductId);

        if (product == null)
            return BadRequest("Product does not exist.");

        // 🔥 这里必须 new 一个 order
        var order = new Order
        {
            CustomerId = dto.CustomerId,
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            Total = product.Price * dto.Quantity
        };

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        return Ok(order);
    }

}

