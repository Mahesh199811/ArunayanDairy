using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Data;
using OrderService.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseInMemoryDatabase("OrderDatabase"));

var productServiceUrl =
    builder.Configuration["Services:ProductService"];

builder.Services.AddHttpClient<ProductServiceClient>(client =>
{
    client.BaseAddress = new Uri(productServiceUrl!);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
