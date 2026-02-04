using System.Text.Json;
using System.Text.Json.Serialization;
using ApplicationCore;
using eShopX.Common.Exceptions.Handlers;
using eShopX.Common.Logging;
using eShopX.Common.Mapping;
using eShopX.Common.Proxy;
using Infrastructure;
using Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpoints();

builder.Services.ConfigureServices(builder.Configuration);

builder.Services.AddMediator(typeof(IAssemblyMarker).Assembly)
    .AddValidators(typeof(IAssemblyMarker).Assembly)
    .AddValidationBehavior()
    .AddLoggingBehavior();

builder.Services.AddMapping(typeof(IAssemblyMarker).Assembly, typeof(Dependencies).Assembly);
builder.Services.DecorateWithDispatchProxyFromAttributes(new[] { typeof(Dependencies).Assembly });

builder.Services.AddProblemDetails();

builder.Services
    .AddExceptionHandler<NotFoundExceptionHandler>()
    .AddExceptionHandler<ValidationExceptionHandler>()
    .AddExceptionHandler<ConflictExceptionHandler>()
    .AddExceptionHandler<ForbiddenExceptionHandler>()
    .AddExceptionHandler<GlobalExceptionHandler>();

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

builder.Services.AddOpenApi();

builder.Logging.ClearProviders()
    .AddCustomLogger(options =>
{
    options.EnableConsole = true;
    options.EnableFile = true;
    options.EnableDb = false;
    options.LogDirectory = "Logs";
    options.FilePrefix = "app-";
    options.MinLevel = LogLevel.Information;
});

var app = builder.Build();

using var scope = app.Services.CreateScope(); 
var db = scope.ServiceProvider.GetRequiredService<EShopContext>();
await DbInitializer.SeedAsync(db);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.UseHttpsRedirection();
app.UseScopeId();
app.UseCors("MyPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapEndpoints();
app.UseExceptionHandler();

app.Run();
