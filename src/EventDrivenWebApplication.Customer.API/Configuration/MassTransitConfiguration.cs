using EventDrivenWebApplication.API.Consumers;
using MassTransit;

namespace EventDrivenWebApplication.API.Configuration;

public static class MassTransitConfiguration
{
    public static void AddMassTransitWithRabbitMq(this IServiceCollection services, string rabbitMqHost)
    {
        services.AddMassTransit(config =>
        {
            config.AddConsumer<ProductCreatedConsumer>();

            config.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqHost);

                cfg.ReceiveEndpoint("product-created-queue", e =>
                {
                    e.ConfigureConsumer<ProductCreatedConsumer>(context);
                });
            });
        });

        services.AddMassTransitHostedService();
    }
}