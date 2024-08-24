using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using EventDrivenWebApplication.Inventory.API.Data;


WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ProductDBContext>(options => options.UseSqlite($"Data Source=product.db"));

//MassTransit
builder.Services.AddMassTransit(options =>
{
    options.SetKebabCaseEndpointNameFormatter();

    options.SetInMemorySagaRepositoryProvider();

    Assembly? entryAssembly = Assembly.GetEntryAssembly();
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

WebApplication? app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    using IServiceScope? scope = app.Services.CreateScope();
    ProductDBContext? dbContext = scope.ServiceProvider.GetRequiredService<ProductDBContext>();
    dbContext.Database.EnsureCreated();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
