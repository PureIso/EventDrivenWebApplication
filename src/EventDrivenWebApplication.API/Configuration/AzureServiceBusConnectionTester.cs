using Azure.Messaging.ServiceBus;

namespace EventDrivenWebApplication.API.Configuration;

public class AzureServiceBusConnectionTester
{
    private readonly ServiceBusClient _serviceBusClient;

    public AzureServiceBusConnectionTester(ServiceBusClient serviceBusClient)
    {
        _serviceBusClient = serviceBusClient;
    }

    public async Task TestConnection(string queueName)
    {
        try
        {
            ServiceBusSender sender = _serviceBusClient.CreateSender(queueName);
            await sender.SendMessageAsync(new ServiceBusMessage("Connection test message"));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to connect to Azure Service Bus queue '{queueName}'.", ex);
        }
    }
}
