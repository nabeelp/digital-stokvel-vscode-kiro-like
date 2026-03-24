namespace DigitalStokvel.Infrastructure.Messaging;

/// <summary>
/// Message bus interface for publishing events
/// </summary>
public interface IMessageBus
{
    /// <summary>
    /// Publish a message to a queue/topic
    /// </summary>
    Task PublishAsync<T>(string queueName, T message, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Publish multiple messages to a queue/topic
    /// </summary>
    Task PublishBatchAsync<T>(string queueName, IEnumerable<T> messages, CancellationToken cancellationToken = default) where T : class;
}

/// <summary>
/// Message consumer interface for receiving messages
/// </summary>
public interface IMessageConsumer
{
    /// <summary>
    /// Start consuming messages from a queue
    /// </summary>
    Task StartAsync(string queueName, Func<string, Task> messageHandler, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop consuming messages
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);
}
