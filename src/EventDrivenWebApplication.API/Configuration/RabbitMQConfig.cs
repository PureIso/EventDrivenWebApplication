namespace EventDrivenWebApplication.API.Configuration;

/// <summary>
/// Configuration settings for connecting to RabbitMQ.
/// </summary>
public class RabbitMqConfig
{
    /// <summary>
    /// Gets or sets the hostname of the RabbitMQ server.
    /// </summary>
    public string Host { get; set; } = default!;

    /// <summary>
    /// Gets or sets the port number of the RabbitMQ server.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets the username for authenticating with RabbitMQ.
    /// </summary>
    public string Username { get; set; } = default!;

    /// <summary>
    /// Gets or sets the password for authenticating with RabbitMQ.
    /// </summary>
    public string Password { get; set; } = default!;
}
