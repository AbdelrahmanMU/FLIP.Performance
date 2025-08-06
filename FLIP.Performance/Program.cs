using FLIP.API.BackgroundJobs;
using FLIP.Application;
using FLIP.Application.Config;
using FLIP.Infrastructure;
using FLIP.Infrastructure.Consumers;
using Hangfire;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Register Serilog as the default logger
Log.Information("Application Starting...");

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // Add JWT Bearer Auth definition
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Example: \"Bearer {your_token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Apply Bearer token to all endpoints by default
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

var key = Encoding.ASCII.GetBytes(builder.Configuration["jwtKey"] ?? "");

var rabbitmqSettings = builder.Configuration.GetSection("RabbitMqSettings").Get<RabbitMqSettings>() ?? new RabbitMqSettings();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = false // disables expiration check
    };
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<FLIPRealTimeConsumer>();
    x.AddConsumer<DailyJobConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host($"rabbitmq://{rabbitmqSettings?.HostName}", h =>
        {
            h.Username(rabbitmqSettings!.UserName);
            h.Password(rabbitmqSettings!.Password);
        });

        cfg.ReceiveEndpoint(rabbitmqSettings!.FLIPRealtimeQeueu, e =>
        {
            e.UseMessageRetry(r => r.Interval(rabbitmqSettings.RetryCount, TimeSpan.FromSeconds(10)));
            e.ConfigureConsumer<FLIPRealTimeConsumer>(context);
        });

        cfg.ReceiveEndpoint(rabbitmqSettings!.DailyJobQeueu, e =>
        {
            e.UseMessageRetry(r => r.Interval(rabbitmqSettings.RetryCount, TimeSpan.FromSeconds(10)));
            e.ConfigureConsumer<DailyJobConsumer>(context);
        });
    });
});

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMqSettings"));

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
