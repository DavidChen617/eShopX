using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Asp.Versioning;
using CoreMesh.Dispatching.Extensions;
using CoreMesh.Endpoints.Extensions;
using CoreMesh.Mapper.Extensions;
using CoreMesh.Validation.Extensions;
using Infrastructure.Data;
using Infrastructure.Search.Elasticsearch;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpoints([typeof(Program).Assembly])
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1);
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'V";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services
    .AddDispatching([typeof(Application.AssemblyMarker).Assembly])
    .AddCoreMeshMapper([typeof(Application.AssemblyMarker).Assembly])
    .AddValidatable()
    .AddCoreMeshExceptionHandling()
    .AddInfrastructureServices(builder.Configuration);

builder.Services.AddHttpClient();

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
        var frontendDomain = builder.Configuration["Site:FrontendDomain"];

        if (!string.IsNullOrWhiteSpace(frontendDomain))
        {
            policy.WithOrigins(frontendDomain)
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

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            factory: _ => new FixedWindowRateLimiterOptions { PermitLimit = 100, Window = TimeSpan.FromSeconds(1) }));
});

builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var esInit = scope.ServiceProvider.GetRequiredService<EsIndexInitializer>();
    await esInit.EnsureIndexAsync();
    
    await DataSeeder.SeedDataAsync(scope.ServiceProvider);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("MyPolicy");
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.UseCoreMeshExceptionHandling();
app.MapEndpoints();

app.Run();
