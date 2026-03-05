using OrderService.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlite("Data Source=orders.db"));

builder.Services.AddHttpClient<ICustomerClient, CustomerClient>(client =>
{
    client.BaseAddress = new Uri("http://customerservice:8080/");
    // client.BaseAddress = new Uri("http://localhost:5142/");
});

builder.Services.AddHttpClient<IProductClient, ProductClient>(client =>
{
    client.BaseAddress = new Uri("http://productservice:8080/");
    // client.BaseAddress = new Uri("http://localhost:5041/");
});

builder.Services.AddControllers();
builder.Services.AddHttpClient();   // 👈 加在这里

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    db.Database.Migrate();
}
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
