using EventDrivenWebApplication.API.Configuration;
using EventDrivenWebApplication.API.Consumers;
using EventDrivenWebApplication.Domain.Interfaces;
using EventDrivenWebApplication.Infrastructure.Data;
using EventDrivenWebApplication.Infrastructure.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Microsoft.OpenApi.Models;
using System.Reflection;
using EventDrivenWebApplication.Domain.Entities;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true);
builder.Configuration.AddEnvironmentVariables();

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Docker")
{
    builder.Configuration.AddUserSecrets<Program>();
    builder.Configuration.AddEnvironmentVariables();
}

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;                      // Keep property names as-is (default camelCase)
        options.JsonSerializerOptions.WriteIndented = true;                             // Format JSON responses with indentation
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());    // Serialize enums as strings
    });

builder.Services.AddApiVersioning(options =>
{
    // Specify the default API version
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Register services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

// Register the database contexts
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductDb")));

builder.Services.AddDbContext<CustomerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CustomerDb")));

builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("InventoryDb")));

RabbitMQConfig? rabbitMqConfig = builder.Configuration.GetSection("MassTransit:RabbitMQ").Get<RabbitMQConfig>();
if (rabbitMqConfig != null)
{
    builder.Services.AddMassTransit(options =>
    {
        options.SetKebabCaseEndpointNameFormatter();

        // Add consumers
        options.AddConsumer<ProductCreatedConsumer>();
        options.AddConsumer<InventoryCheckRequestedConsumer>();
        options.AddConsumer<InventoryCheckCompletedConsumer>();

        // Configure saga state machine
        //options.AddSagaStateMachine<OrderStateMachine, OrderProcessState>()
        //    .InMemoryRepository();

        options.UsingRabbitMq((context, config) =>
        {
            config.Host(new Uri($"rabbitmq://{rabbitMqConfig.Host}:{rabbitMqConfig.Port}/"), hostConfig =>
            {
                hostConfig.Username(rabbitMqConfig.Username);
                hostConfig.Password(rabbitMqConfig.Password);
            });

            // Configure endpoints for consumers
            config.ReceiveEndpoint("product-created-queue", e =>
            {
                e.ConfigureConsumer<ProductCreatedConsumer>(context);
            });

            config.ReceiveEndpoint("inventory-check-requested-queue", e =>
            {
                e.ConfigureConsumer<InventoryCheckRequestedConsumer>(context);
            });

            config.ReceiveEndpoint("inventory-check-completed-queue", e =>
            {
                e.ConfigureConsumer<InventoryCheckCompletedConsumer>(context);
            });

            // Configure saga
            config.ReceiveEndpoint("order-process-saga", e =>
            {
                e.ConfigureSaga<OrderProcessState>(context);
            });
        });
    });
}

// Add Swagger for API documentation
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Event Driven Web Application API", Version = "v1" });
    string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event Driven Web Application API v1"));
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

// Ensure the databases are created
using (IServiceScope scope = app.Services.CreateScope())
{
    ProductDbContext productDbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    productDbContext.Database.EnsureCreated();

    CustomerDbContext customerDbContext = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
    customerDbContext.Database.EnsureCreated();

    InventoryDbContext inventoryDbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    inventoryDbContext.Database.EnsureCreated();
}

app.Run();
