using FLIP.API.BackgroundJobs;
using FLIP.Application;
using FLIP.Infrastructure;
using Hangfire;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Register Serilog as the default logger
Log.Information("Application Starting...");

// Add services to the container.
builder.Services.AddHangfire(x => x.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddScoped<RecallingApis>();

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

using (var scope = app.Services.CreateScope())
{
    var recallingApis = scope.ServiceProvider.GetRequiredService<RecallingApis>();
    recallingApis.SchedulingTheJob();
}

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


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseHangfireDashboard("/dashboard");

app.UseAuthorization();

app.MapControllers();

app.Run();
