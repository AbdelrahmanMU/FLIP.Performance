using FLIP.Performance.Config;
using FLIP.Performance.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Register Serilog as the default logger
Log.Information("Application Starting...");

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMqSettings"));

// Register RabbitMqConsumer
builder.Services.AddSingleton<RabbitMqConsumer>();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("../../../logs/API-log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 10,
        fileSizeLimitBytes: 10_000_000,
        rollOnFileSizeLimit: true,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level}] {Message}{NewLine}{Exception}")
    .CreateLogger();

var app = builder.Build();

// Start the RabbitMQ Consumer in the background
var consumer = app.Services.GetRequiredService<RabbitMqConsumer>();
await consumer.Start();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
