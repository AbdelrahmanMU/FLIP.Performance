using FLIP.Application.Config;
using FLIP.Application.Interfaces;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Serilog;
using System.Text;

namespace FLIP.Infrastructure.Services;

public class NotifyMessages(IOptions<RabbitMqSettings> settings) : INotifyMessages
{
    private readonly RabbitMqSettings _settings = settings.Value;
    private readonly ILogger _logger = Log.Logger;

    public async Task NotifyBREAsync(int number)
    {
        var factory = new ConnectionFactory()
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password,
        };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(queue: _settings.QueueName,
                             durable: true,
                             exclusive: false,
                             autoDelete: false);

        try
        {
            var body = Encoding.UTF8.GetBytes(number.ToString());
            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: _settings.QueueName,
                body: body);

            _logger.Information($"[Publisher] Sent: {number}");
        }
        catch (Exception ex)
        {
            _logger.Error(ex.Message);

            throw new Exception(ex.Message);
        }
    }
}
