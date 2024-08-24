using MassTransit;
using System.Reflection;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMassTransit(options =>
{
    //readable
    options.SetKebabCaseEndpointNameFormatter();

    options.SetInMemorySagaRepositoryProvider();
    //scan assembly for any implementation of IConsumer
    Assembly? entryAssembly = Assembly.GetEntryAssembly();
    options.AddConsumers(entryAssembly);

    //https://www.youtube.com/watch?v=8_cvhomwmto&list=PLMZQrdYaj-DWUYo2eVGPBx268hQB7dPrl&index=4&ab_channel=codingFriday
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

IHost host = builder.Build();
host.Run();
