using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Data;
using OrderService.Api.Models;
using OrderService.Api.DTOs;
using OrderService.Services;      // ✅ RabbitMQ
using System.Text.Json;

namespace OrderService.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly OrdersDbContext _context;
    private readonly ICustomerClient _customerClient;
    private readonly IProductClient _productClient;

    // ✅ 老师风格：class-level publisher
    private readonly RabbitMQPublisher _publisher;
    private readonly RabbitMQPublisher _cancelPublisher;

    public OrdersController(
        OrdersDbContext context,
        ICustomerClient customerClient,
        IProductClient productClient)
    {
        _context = context;
        _customerClient = customerClient;
        _productClient = productClient;

        _publisher = new RabbitMQPublisher("rabbitmq", "order_created");
        _cancelPublisher = new RabbitMQPublisher("rabbitmq", "order_cancelled");
        // _publisher = new RabbitMQPublisher("localhost", "order_created");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll()
    {
        var orders = await _context.Orders
            .Select(o => new OrderDto
            {
                Id = o.Id,
                Total = o.Total,
                CustomerId = o.CustomerId,
                ProductId = o.ProductId,
                Quantity = o.Quantity,
                CreatedAt = o.CreatedAt,
                Status = o.Status
            })
            .ToListAsync();

        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        var order = await _context.Orders.FindAsync(id);

        if (order == null)
            return NotFound();

        return Ok(new OrderDto
        {
            Id = order.Id,
            Total = order.Total,
            CustomerId = order.CustomerId,
            ProductId = order.ProductId,
            Quantity = order.Quantity,
            CreatedAt = order.CreatedAt,
            Status = order.Status
        });
    }

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

        var order = new Order
        {
            CustomerId = dto.CustomerId,
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            Total = product.Price * dto.Quantity
        };

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        // ✅ RabbitMQ（老师写法）
        var message = JsonSerializer.Serialize(order);
        _publisher.Publish(message);

        return Ok(new OrderDto
        {
            Id = order.Id,
            Total = order.Total,
            CustomerId = order.CustomerId,
            ProductId = order.ProductId,
            Quantity = order.Quantity,
            CreatedAt = order.CreatedAt,
            Status = order.Status
        });
    }

    // 🔥 取消订单 endpoint
    [HttpDelete("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var order = await _context.Orders.FindAsync(id);

        if (order == null)
            return NotFound();

        if (order.Status == "Cancelled")
            return BadRequest("Order is already cancelled.");

        order.Status = "Cancelled";
        await _context.SaveChangesAsync();

        // 🔥 发布 OrderCancelled 事件
        var message = JsonSerializer.Serialize(order);
        _cancelPublisher.Publish(message);

        Console.WriteLine($"Order {id} cancelled, stock will be restored");

        return Ok(new OrderDto
        {
            Id = order.Id,
            Total = order.Total,
            CustomerId = order.CustomerId,
            ProductId = order.ProductId,
            Quantity = order.Quantity,
            CreatedAt = order.CreatedAt,
            Status = order.Status
        });
    }
}



// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using OrderService.Api.Data;
// using OrderService.Api.Models;
// using OrderService.Api.DTOs;

// namespace OrderService.Api.Controllers;

// [ApiController]
// [Route("api/orders")]
// public class OrdersController : ControllerBase
// {
//     private readonly OrdersDbContext _context;
//     private readonly ICustomerClient _customerClient;
//     private readonly IProductClient _productClient;

//     public OrdersController(
//         OrdersDbContext context,
//         ICustomerClient customerClient,
//         IProductClient productClient)
//     {
//         _context = context;
//         _customerClient = customerClient;
//         _productClient = productClient;
//     }

//     [HttpGet]
//     public async Task<IActionResult> GetAll()
//     {
//         return Ok(await _context.Orders.ToListAsync());
//     }

//     [HttpGet("{id}")]
//     public async Task<IActionResult> GetById(int id)
//     {
//         var order = await _context.Orders.FindAsync(id);

//         if (order == null)
//             return NotFound();

//         return Ok(order);
//     }


//     [HttpPost]
//     public async Task<IActionResult> Create(CreateOrderDto dto)
//     {
//         var customerExists =
//             await _customerClient.CustomerExistsAsync(dto.CustomerId);

//         if (!customerExists)
//             return BadRequest("Customer does not exist.");

//         var product =
//             await _productClient.GetProductAsync(dto.ProductId);

//         if (product == null)
//             return BadRequest("Product does not exist.");

//         // 🔥 这里必须 new 一个 order
//         var order = new Order
//         {
//             CustomerId = dto.CustomerId,
//             ProductId = dto.ProductId,
//             Quantity = dto.Quantity,
//             Total = product.Price * dto.Quantity
//         };

//         await _context.Orders.AddAsync(order);
//         await _context.SaveChangesAsync();

//         return Ok(order);
//     }

// }

