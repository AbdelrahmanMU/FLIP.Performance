using FLIP.Performance.Config;
using FLIP.Performance.Consumers;
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

builder.Services.AddSingleton<IDapperQueries, DapperQueries>();

// Register RabbitMqConsumer
builder.Services.AddSingleton<RabbitMqConsumer>();
builder.Services.AddMemoryCache();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/API-log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 10,
        fileSizeLimitBytes: 10_000_000,
        rollOnFileSizeLimit: true,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level}] {Message}{NewLine}{Exception}")
    .CreateLogger();

var app = builder.Build();

app.Use(async (context, next) =>
{
    try
    {
        await next(); // Continue processing
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Unhandled exception occurred!");

        // Respond with a generic error message
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("An unexpected error occurred.");
    }
});

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
