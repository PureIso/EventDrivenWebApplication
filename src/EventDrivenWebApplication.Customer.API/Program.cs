using EventDrivenWebApplication.API.Configuration;
using EventDrivenWebApplication.Domain.Interfaces;
using EventDrivenWebApplication.Infrastructure.Data;
using EventDrivenWebApplication.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register the ProductService as a dependency
builder.Services.AddScoped<IProductService, ProductService>();

// Register the database context
builder.Services.AddDbContext<CustomerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register MassTransit with RabbitMQ configuration
builder.Services.AddMassTransitWithRabbitMq(builder.Configuration["RabbitMqHost"]);

builder.Services.AddControllers();

// Add Swagger/OpenAPI support if needed
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Ensure the database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
    dbContext.Database.EnsureCreated();
}

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