using Microsoft.EntityFrameworkCore;
using PaymentService.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient();

builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlite("Data Source=payments.db"));

builder.Services.AddHttpClient<IOrderClient, OrderClient>(client =>
{
    client.BaseAddress = new Uri("http://orderservice:8080/");
    // client.BaseAddress = new Uri("http://localhost:5246/");
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();