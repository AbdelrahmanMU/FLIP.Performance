﻿namespace FLIP.Application.Config;

public class RabbitMqSettings
{
    public string HostName { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;
    public int Port { get; set; }
    public string QueueName { get; set; } = default!;
}