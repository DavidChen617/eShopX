using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using eShopX.Common.Exceptions.Handlers;
using eShopX.Common.Logging;
using eShopX.Common.Mapping;
using eShopX.Common.Proxy;
using Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpoints();

builder.Services.ConfigureInfrastructureServices(builder.Configuration);

builder.Services.AddMediator(typeof(IAssemblyMarker).Assembly)
    .AddValidators(typeof(IAssemblyMarker).Assembly)
    .AddValidationBehavior()
    .AddLoggingBehavior();

builder.Services.AddMapping(typeof(IAssemblyMarker).Assembly, typeof(Dependencies).Assembly);

builder.Services.AddProblemDetails();

builder.Services
    .AddExceptionHandler<NotFoundExceptionHandler>()
    .AddExceptionHandler<ValidationExceptionHandler>()
    .AddExceptionHandler<ConflictExceptionHandler>()
    .AddExceptionHandler<ForbiddenExceptionHandler>()
    .AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.DecorateWithDispatchProxyFromAttributes([typeof(Dependencies).Assembly, typeof(IAssemblyMarker).Assembly]);

builder.Services.AddHttpClient();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IScopeIdAccessor, ScopeIdAccessor>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
});

builder.Services.ConfigureHttpJsonOptions(opt =>
{
    opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    opt.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "MyPolicy", policy =>
    {
        var origins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>();

        if (origins is { Length: > 0 })
        {
            policy.WithOrigins(origins)
                .AllowAnyMethod()
                .AllowAnyHeader();
            return;
        }

        if (builder.Environment.IsDevelopment())
        {
            policy.WithOrigins("http://localhost:4200")
                .AllowAnyMethod()
                .AllowAnyHeader();
            return;
        }

        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Rate Limiting - Current limiting and anti-brushing
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Global current limit: 100 times per second per IP
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            factory: _ => new FixedWindowRateLimiterOptions { PermitLimit = 100, Window = TimeSpan.FromSeconds(1) }));

    // Flash Sale only: 5 per second per user
    options.AddPolicy("FlashSale", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            factory: _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromSeconds(1) }));
});

builder.Services.AddOpenApi();

builder.Logging.ClearProviders()
    .AddCustomLogger<PostgresLogDbProvider>(options =>
    {
        options.EnableConsole = true;
        options.EnableFile = true;
        options.EnableDb = true;
        options.LogDirectory = "Logs";
        options.FilePrefix = "app-";
        options.MinLevel = LogLevel.Information;
        options.IncludeThreadId = false;
        options.IncludeSinkThreadId = false;
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<EShopContext>();
    await DbInitializer.SeedAsync(db);

    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.UseHttpsRedirection();
app.UseScopeId();
app.UseCors("MyPolicy");
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.MapEndpoints();
app.UseExceptionHandler();

app.Run();
