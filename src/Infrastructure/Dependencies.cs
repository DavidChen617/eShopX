using System.Security.Claims;
using System.Text;

using CloudinaryDotNet;

using eShopX.Common.Proxy;

using Infrastructure.Auth;
using Infrastructure.Auth.ThirdPartyAuth;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Infrastructure.Options;
using Infrastructure.Payments;
using Infrastructure.Payments.Line;
using Infrastructure.Payments.Line.Models;
using Infrastructure.Payments.PayPal;
using Infrastructure.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using StackExchange.Redis;

namespace Infrastructure;

public static class Dependencies
{
    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<EShopContext>((sp, options) =>
        {
            options.UseNpgsql(
                    configuration.GetConnectionString(nameof(ConnectionStrings.PostgreSQL)))
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
        });

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>));

        // Redis
        var options = ConfigurationOptions.Parse(
            configuration.GetConnectionString(nameof(ConnectionStrings.Redis))!);
        options.AbortOnConnectFail = false;
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(options));

        // services.AddSingleton<IConnectionMultiplexer>(_ =>
        // ConnectionMultiplexer.Connect(configuration.GetConnectionString(nameof(ConnectionStrings.Redis))!));
        services.AddScoped<ICacheService, RedisCacheService>();

        // Jwt
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.OptionKey));
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtService, JwtService>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var jwtOptions = configuration.GetSection(JwtOptions.OptionKey).Get<JwtOptions>()!;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                    RoleClaimType = ClaimTypes.Role
                };
            });

        // Cloudinary
        services.Configure<CloudinaryOptions>(configuration.GetSection(CloudinaryOptions.OptionKey));
        services.AddSingleton<Cloudinary>(_ =>
        {
            var opt = configuration.GetSection(CloudinaryOptions.OptionKey).Get<CloudinaryOptions>()
                ?? throw new InvalidOperationException("Cloudinary configuration is missing.");
            return new Cloudinary(new Account(opt.CloudName, opt.ApiKey, opt.ApiSecret));
        });
        services.AddScoped<IImageStorage, ImageStorageService>();

        // Email
        services.Configure<MailOptions>(configuration.GetSection(MailOptions.OptionKey));
        services.AddScoped<IMailSender, MailKitEmailSender>();
        services.Configure<GoogleAuthOptions>(configuration.GetSection(GoogleAuthOptions.OptionKey));
        services.Configure<LineAuthOptions>(configuration.GetSection(LineAuthOptions.OptionKey));

        // Google Auth
        services.AddKeyedScoped<IThirdPartyAuthService<GoogleAuthRequest, GoogleAuthResponse>, GoogleAuthService>(GoogleAuthOptions.OptionKey);

        // Line Auth
        services.AddKeyedScoped<IThirdPartyAuthService<LineAuthRequest, LineAuthResponse>, LineAuthService>(LineAuthOptions.OptionKey);

        // LinePay
        services.Configure<LinePayOptions>(configuration.GetSection(LinePayOptions.OptionKey));
        services
            .AddScoped<IPaymentService<LinePayRequest, LinePayRequestResponse?, LinePayConfirmInput,
                LinePayConfirmResponse?>, LinePayService>();

        // PayPal
        services.Configure<PayPalOptions>(configuration.GetSection(PayPalOptions.OptionKey));
        services
            .AddScoped<IPaymentService<PayPalCreateOrderRequest, PayPalCreateOrderResponse, PayPalCaptureRequest,
                PayPalCaptureOrderResponse>, PayPalService>();

        services.DecorateWithDispatchProxyFromAttributes(new[] { typeof(Dependencies).Assembly });
    }
}