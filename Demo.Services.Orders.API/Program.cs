using Demo.Services.Orders.API.Data;
using Demo.Services.Orders.API.Messaging;
using Demo.Services.Orders.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// 1. Local Database Context Registration
builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrdersConnection")));

// 2. HTTP Clients Registration pointing to your exact active microservice ports
builder.Services.AddHttpClient<CustomerHttpService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7180"); // Customer API port
});

builder.Services.AddHttpClient<CatalogHttpService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7088"); // Catalog API port
});


//Register the Background Worker --tell the Web API engine to run this background listener
//worker immediately on startup
builder.Services.AddHostedService<RabbitMQProductConsumer>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
