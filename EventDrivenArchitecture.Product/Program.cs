using EventDrivenArchitecture.Inventory.Data;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Reflection;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ProductDBContext>(options => options.UseSqlite($"Data Source=product.db"));

//MassTransit
builder.Services.AddMassTransit(options =>
{
    options.SetKebabCaseEndpointNameFormatter();

    options.SetInMemorySagaRepositoryProvider();

    var entryAssembly = Assembly.GetEntryAssembly();
    options.AddSagaStateMachines(entryAssembly);
    options.AddSagas(entryAssembly);
    options.AddActivities(entryAssembly);

    options.UsingRabbitMq((context, config) =>
    {
        config.Host("localhost", "/", hostConfig =>
        {
            hostConfig.Username("guest");
            hostConfig.Password("guest");
        });
        config.ConfigureEndpoints(context);
    });
});

https://www.youtube.com/watch?v=NZeS9R1VLnM&ab_channel=CsharpSpace
//FluentValidation
//builder.Services.AddScoped<IValidator<ProductCreated>, ProductCreatedValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Singleton);


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductDBContext>();
    dbContext.Database.EnsureCreated();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
