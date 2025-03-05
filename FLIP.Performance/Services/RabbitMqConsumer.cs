using FLIP.Performance.Config;
using FLIP.Performance.Utilities;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using System.Text;

namespace FLIP.Performance.Services;

public class RabbitMqConsumer(IOptions<RabbitMqSettings> options)
{
    private readonly RabbitMqSettings _settings = options.Value;
    private readonly ApiCaller _apiCaller = new();
    private readonly Serilog.ILogger _logger = Log.Logger;

    public async Task Start()
    {
        try
        {
            var factory = new ConnectionFactory()
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password,
                Uri = new Uri($"amqp://{_settings.UserName}:{_settings.Password}@{_settings.HostName}:{_settings.Port}"),
                RequestedHeartbeat = TimeSpan.FromSeconds(30),
                AutomaticRecoveryEnabled = true,
            };

            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();


            await channel.QueueDeclareAsync(queue: _settings.QueueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: new Dictionary<string, object?>
                                 {
                                     { "x-queue-mode", "default" } // Avoid "lazy"
                                 });

            await channel.QueueDeclareAsync(queue: _settings.DeadLetterQueue,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: new Dictionary<string, object?>
                                 {
                                     { "x-queue-mode", "default" } // Avoid "lazy"
                                 });

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var isParsed = int.TryParse(message, out int id);

                    if (isParsed)
                    {
                        // At least one success
                        var (success, apiLogs, freelancerDataResponse) = await _apiCaller.ExecuteParallelApiCallsAsync(id);
                        if (success)
                        {
                            _logger.Information("[Consumer] Message processed successfully", message);

                            try
                            {
                                // Is ack impact the performance?
                                await channel.BasicAckAsync(ea.DeliveryTag, true);
                            }
                            catch (Exception ex)
                            {
                                // Requeue the message if processing fails
                                await channel.BasicNackAsync(ea.DeliveryTag, false, true);

                                _logger.Error($"[Consumer] Error in Start method: {ex.Message}", ex.Message);
                            }
                        }
                        else
                        {
                            _logger.Warning("[Consumer] Invalid message format. Moving to DLQ", message);
                            await SendToDeadLetterQueue(channel, body);
                        }
                    }
                    else
                    {
                        _logger.Warning("[Consumer] Invalid message format. Moving to DLQ", message);
                        await SendToDeadLetterQueue(channel, body);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("[Consumer] Error processing message", ex.Message);
                    await SendToDeadLetterQueue(channel, ea.Body.ToArray());
                }
            };

            await channel.BasicConsumeAsync(queue: _settings.QueueName, autoAck: false, consumer: consumer);
            _logger.Information("[Consumer] Waiting for messages...");

            // Keep the application alive
            var tcs = new TaskCompletionSource();
            await tcs.Task;
        }
        catch (Exception ex)
        {
            _logger.Error($"[Consumer] Error in Start method: {ex.Message}", ex.Message);
        }
    }

    private async Task SendToDeadLetterQueue(IChannel channel, byte[] messageBody)
    {
        try
        {
            await channel.BasicPublishAsync(exchange: "",
                                 routingKey: _settings.DeadLetterQueue,
                                 body: messageBody);

            _logger.Information("[Consumer] Moved message to DLQ.");
        }
        catch (Exception ex)
        {
            _logger.Error($"[Consumer] Failed to send message to DLQ: {ex.Message}", ex.Message);
        }
    }
}
