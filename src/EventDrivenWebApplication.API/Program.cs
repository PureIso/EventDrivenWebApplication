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
using EventDrivenWebApplication.Infrastructure.Sagas;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Azure.Messaging.ServiceBus;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Check if running in Docker
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Docker")
{
    builder.Configuration.AddUserSecrets<Program>();
    builder.Configuration.AddEnvironmentVariables();
}

// Configure JSON serialization options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddApiVersioning(options =>
{
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
builder.Services.AddScoped<IOrderProcessStateService, OrderProcessStateService>();

// Register the database contexts
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductDb")));

builder.Services.AddDbContext<CustomerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CustomerDb")));

builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("InventoryDb")));

builder.Services.AddDbContext<OrderSagaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrderSagaStateDb")));

string? transport = builder.Configuration.GetSection("MassTransit:Transport").Value;
if (transport == "AzureServiceBus")
{
        // Extract the connection string from configuration
        string azureServiceBusConnectionString = builder.Configuration["MassTransit:AzureServiceBus:ConnectionString"];

        builder.Services.AddSingleton(serviceProvider => new ServiceBusClient(azureServiceBusConnectionString));
        builder.Services.AddSingleton<AzureServiceBusConnectionTester>();

        //AzureServiceBusConfig? azureServiceBusConfig = builder.Configuration.GetSection("MassTransit:AzureServiceBus").Get<AzureServiceBusConfig>();
        //Log.Information($"Azure Service Bus Connection String: {azureServiceBusConfig.ConnectionString}");
        //options.UsingAzureServiceBus((context, config) =>
        //{
        //    config.Host(azureServiceBusConfig.ConnectionString, h =>
        //    {
        //        h.TransportType = ServiceBusTransportType.AmqpWebSockets;
        //    });

        //    // Configure consumers and queues
        //    config.ReceiveEndpoint("product-created-queue", e =>
        //    {
        //        e.ConfigureConsumer<ProductCreatedConsumer>(context);
        //    });
        //    config.ReceiveEndpoint("inventory-check-requested-queue", e =>
        //    {
        //        e.ConfigureConsumer<InventoryCheckRequestedConsumer>(context);
        //    });
        //    config.ReceiveEndpoint("inventory-check-completed-queue", e =>
        //    {
        //        e.ConfigureConsumer<InventoryCheckCompletedConsumer>(context);
        //    });
        //    config.ReceiveEndpoint("order-process-saga", e =>
        //    {
        //        e.ConfigureSaga<OrderProcessState>(context);
        //    });
        //});
}
else if (transport == "RabbitMQ")
{
    builder.Services.AddMassTransit(options =>
    {
        options.SetKebabCaseEndpointNameFormatter();

        // Add consumers for the events
        options.AddConsumer<ProductCreatedConsumer>();
        options.AddConsumer<InventoryCheckRequestedConsumer>();
        options.AddConsumer<InventoryCheckCompletedConsumer>();

        // Configure the Saga state machine
        options.AddSagaStateMachine<OrderProcessStateMachine, OrderProcessState>()
            .EntityFrameworkRepository(r =>
            {
                r.ExistingDbContext<OrderSagaDbContext>();
                r.UseSqlServer();
            });

        RabbitMqConfig? rabbitMqConfig = builder.Configuration.GetSection("MassTransit:RabbitMQ").Get<RabbitMqConfig>();
        options.UsingRabbitMq((context, config) =>
        {
            config.Host(new Uri($"rabbitmq://{rabbitMqConfig.Host}:{rabbitMqConfig.Port}/"), hostConfig =>
            {
                hostConfig.Username(rabbitMqConfig.Username);
                hostConfig.Password(rabbitMqConfig.Password);
            });

            // Configure consumers and queues
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
            config.ReceiveEndpoint("order-process-saga", e =>
            {
                e.ConfigureSaga<OrderProcessState>(context);
            });
        });

    });
}
else
{
    throw new InvalidOperationException($"Unsupported transport type: {transport}");
}
  


// Add Swagger for API documentation
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Event Driven Web Application API", Version = "v1" });
    string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment() || builder.Environment.EnvironmentName == "Docker")
{
    app.UseCors("AllowAll");
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event Driven Web Application API v1"));
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

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

    OrderSagaDbContext orderSagaDbContext = scope.ServiceProvider.GetRequiredService<OrderSagaDbContext>();
    orderSagaDbContext.Database.EnsureCreated();
}

// Logging Kestrel and HTTPS certificate information
ILogger<Program> logger = app.Services.GetRequiredService<ILogger<Program>>();
IServer server = app.Services.GetRequiredService<IServer>();
ICollection<string>? addresses = server.Features.Get<IServerAddressesFeature>()?.Addresses;
AzureServiceBusConnectionTester connectionTester = app.Services.GetRequiredService<AzureServiceBusConnectionTester>();

if (addresses != null && addresses.Any())
{
    foreach (string address in addresses)
    {
        logger.LogInformation("Kestrel is listening on: {Address}", address);
    }
}
else
{
    logger.LogInformation("Kestrel addresses are empty.");
}
try
{
    logger.LogInformation(builder.Configuration["MassTransit:AzureServiceBus:ConnectionString"]);
    string queueName = "product-created-queue";
    connectionTester.TestConnection(queueName).GetAwaiter().GetResult();
    logger.LogInformation("Azure Service Bus connection test succeeded. Queue '{QueueName}' is accessible.", queueName);
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Azure Service Bus connection test failed. Application will shut down.");
    Environment.Exit(1);
}


// Log application startup
logger.LogInformation("Application has started successfully.");

app.Run();
