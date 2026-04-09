using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CustomerService.Api.Data;
using CustomerService.Api.Models;
using CustomerService.Api.DTOs;

namespace CustomerService.Api.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly CustomerDbContext _context;

    public CustomersController(CustomerDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> List()
    {
        var customers = await _context.Customers
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email
            })
            .ToListAsync();

        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetById(int id)
    {
        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
            return NotFound();

        return Ok(new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email
        });
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(CustomerDto dto)
    {
        var customer = new Customer
        {
            Name = dto.Name,
            Email = dto.Email
        };

        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();

        var response = new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email
        };

        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, response);
    }
}

