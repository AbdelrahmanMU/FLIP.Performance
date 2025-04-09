using FLIP.API.Config;
using FLIP.API.Services;
using FLIP.API.Utilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using System.Diagnostics;
using System.Text;

namespace FLIP.API.Consumers;

public class RabbitMqConsumer(IOptions<RabbitMqSettings> options,
    IDapperQueries dapperQueries,
    IMemoryCache memoryCache,
    IConfiguration configuration)
{
    private readonly RabbitMqSettings _settings = options.Value;
    private readonly ApiCaller _apiCaller = new(configuration);
    private readonly Serilog.ILogger _logger = Log.Logger;
    private readonly IDapperQueries _dapperQueries = dapperQueries;
    private readonly IMemoryCache _memoryCache = memoryCache;

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

            await channel.QueueDeclareAsync(queue: _settings.DeadLetterQueue,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false);

            var consumer = new AsyncEventingBasicConsumer(channel);

            var stopwatch = Stopwatch.StartNew();

            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var isParsed = int.TryParse(message, out int id);

                    if (isParsed)
                    {
                        if (_memoryCache.TryGetValue(id, out _))
                        {
                            // ID is already cached
                            await channel.BasicNackAsync(ea.DeliveryTag, false, false);
                            return;
                        }

                        // ID is not cached, so we add it
                        _memoryCache.Set(id, true);

                        // At least one success
                        var (success, apiLogs, freelancerDataResponse, errorLogs) = await _apiCaller.ExecuteParallelApiCallsAsync(id);
                        if (success)
                        {
                            double resposdnseTimeMs = stopwatch.Elapsed.TotalMilliseconds;

                            await _dapperQueries.InsertLogs(apiLogs);
                            await _dapperQueries.InsertFreelancers(freelancerDataResponse);
                            await _dapperQueries.InsertFreelancersRide(freelancerDataResponse);
                            await _dapperQueries.InsertErrorLogs(errorLogs);

                            _logger.Information("[Consumer] Message processed successfully", message);

                            try
                            {
                                // Is ack impact the performance?
                                await channel.BasicAckAsync(ea.DeliveryTag, true);
                            }
                            catch (Exception ex)
                            {
                                // Requeue the message if processing fails
                                await channel.BasicNackAsync(ea.DeliveryTag, false, false);

                                _logger.Error($"[Consumer] Error in Start method: {ex.Message}", ex.Message);
                            }
                        }
                        else
                        {
                            _logger.Warning("[Consumer] Invalid message format. Moving to DLQ", message);
                            await channel.BasicNackAsync(ea.DeliveryTag, false, false);
                            await SendToDeadLetterQueue(channel, body);
                        }
                    }
                    else
                    {
                        _logger.Warning("[Consumer] Invalid message format. Moving to DLQ", message);
                        await channel.BasicNackAsync(ea.DeliveryTag, false, false);
                        await SendToDeadLetterQueue(channel, body);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("[Consumer] Error processing message", ex.Message);
                    await channel.BasicNackAsync(ea.DeliveryTag, false, false);
                    await SendToDeadLetterQueue(channel, ea.Body.ToArray());
                }

                stopwatch.Stop();
                double responseTimeMs = stopwatch.Elapsed.TotalMilliseconds;

                var x = 10;
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
