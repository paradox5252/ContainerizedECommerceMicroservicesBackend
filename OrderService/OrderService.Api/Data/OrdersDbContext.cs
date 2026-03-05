using Microsoft.EntityFrameworkCore;
using OrderService.Api.Models;

namespace OrderService.Api.Data;

public class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options)
        : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
}

// private readonly OrderDbContext _context;
// private readonly HttpClient _httpClient;

// public OrdersController(OrderDbContext context, IHttpClientFactory factory)
// {
//     _context = context;
//     _httpClient = factory.CreateClient();
// }