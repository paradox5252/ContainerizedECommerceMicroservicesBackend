using Microsoft.EntityFrameworkCore;
using ProductService.Api.Data;
using ProductService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlite("Data Source=products.db"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    db.Database.Migrate();
}

var consumer = new RabbitMQConsumer(app.Services);
consumer.Start();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();


app.Run();