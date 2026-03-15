using System.Security.Claims;
using System.Text;
using CloudinaryDotNet;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using eShopX.Application.Interfaces;
using eShopX.Application.Interfaces.Repositories;
using Infrastructure.Auth;
using Infrastructure.Auth.ThirdPartyAuth;
using Infrastructure.Caches;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using eShopX.Application.Interfaces.Repositories;
using Infrastructure.Email;
using Infrastructure.Logistics;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Orders;
using Infrastructure.Messaging.Products;
using Infrastructure.Payments;
using Infrastructure.Payments.Line;
using Infrastructure.Payments.PayPal;
using Infrastructure.Search.Elasticsearch;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace Infrastructure;

public static class Dependencies
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Site
        services.Configure<SiteOptions>(configuration.GetSection(SiteOptions.OptionKey));

        // Database
        services.AddDbContext<EShopContext>((_, options) =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString(nameof(ConnectionStrings.PostgreSQL)),
                npgsql =>
                {
                    npgsql.MinBatchSize(1);
                    npgsql.MaxBatchSize(100);
                    npgsql.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                })
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
        });

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        // Repositories
        services
            .AddScoped<IOutboxEventRepository, OutboxEventRepository>()
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IRefreshTokenRepository, RefreshTokenRepository>()
            .AddScoped<ICartRepository, CartRepository>()
            .AddScoped<IProductRepository, ProductRepository>()
            .AddScoped<IOrderRepository, OrderRepository>()
            .AddScoped<IPaymentRepository, PaymentRepository>()
            .AddScoped<IShipmentRepository, ShipmentRepository>()
            .AddScoped<ISizeRepository, SizeRepository>();

        // Redis
        var redisOptions = ConfigurationOptions.Parse(
            configuration.GetConnectionString(nameof(ConnectionStrings.Redis))!);
        redisOptions.AbortOnConnectFail = false;
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisOptions));
        services.AddScoped<ICacher, RedisCacher>();

        // JWT
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.OptionKey));
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                var jwtOptions = configuration.GetSection(JwtOptions.OptionKey).Get<JwtOptions>()!;
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
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
        services.AddScoped<IImageStorage, ImageStorage>();

        // Email
        services.Configure<MailOptions>(configuration.GetSection(MailOptions.OptionKey));
        services.AddScoped<IMailSender, MailKitEmailSender>();

        // Google Auth
        services.Configure<GoogleAuthOptions>(configuration.GetSection(GoogleAuthOptions.OptionKey));
        services.AddScoped<IThirdPartyAuthService<GoogleAuthRequest, GoogleAuthResponse>, GoogleAuthService>();

        // LINE Auth
        services.Configure<LineAuthOptions>(configuration.GetSection(LineAuthOptions.OptionKey));
        services.AddScoped<IThirdPartyAuthService<LineAuthRequest, LineAuthResponse>, LineAuthService>();

        // LinePay
        services.Configure<LinePayOptions>(configuration.GetSection(LinePayOptions.OptionKey));
        services.AddScoped<LinePayService>();

        // PayPal
        services.Configure<PayPalOptions>(configuration.GetSection(PayPalOptions.OptionKey));
        services.AddHttpClient<PayPalClient>((sp, client) =>
        {
            var opt = sp.GetRequiredService<IOptions<PayPalOptions>>().Value;
            client.BaseAddress = new Uri(opt.BaseUrl);
        });
        services.AddScoped<PayPalService>();
        services.AddScoped<IPaymentGateway, PaymentGateway>();

        // ECPay
        services.Configure<ECPayOptions>(configuration.GetSection(ECPayOptions.OptionKey));
        services.AddScoped<IECPayLogisticsService, ECPayLogisticsService>();

        // Kafka
        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.OptionKey));
        services.AddSingleton<IProducer<string, string>>(sp =>
        {
            var kafkaOptions = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;
            return new ProducerBuilder<string, string>(kafkaOptions.Producer).Build();
        });
        services.AddScoped<IConsumer<string, string>>(sp =>
        {
            var kafkaOptions = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;
            return new ConsumerBuilder<string, string>(kafkaOptions.Consumer).Build();
        });
        services.AddSingleton<IOutboxEventPublisher, ProductIndexOutboxEventPublisher>();
        services.AddSingleton<AdminClientConfig>(sp =>
        {
            var kafkaOptions = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;
            return new AdminClientConfig { BootstrapServers = kafkaOptions.Producer.BootstrapServers };
        });
        services.AddSingleton<List<TopicSpecification>>(sp =>
        {
            var kafkaOptions = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;
            return [new TopicSpecification { Name = kafkaOptions.OutboxEventTopic, NumPartitions = 3, ReplicationFactor = 1 }];
        });
        services.AddSingleton<IAdminClient>(sp =>
            new AdminClientBuilder(sp.GetRequiredService<AdminClientConfig>()).Build());

        services
            .AddHostedService<MessageTopicInitializer>()
            .AddHostedService<OutboxPublisherHostedService>()
            .AddHostedService<OutboxConsumerHostedService>();

        // Elasticsearch
        services.Configure<ElasticsearchOptions>(configuration.GetSection(ElasticsearchOptions.OptionKey));
        services.AddSingleton(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<ElasticsearchOptions>>().Value;
            var settings = new ElasticsearchClientSettings(new Uri(opt.Url));
            if (!string.IsNullOrWhiteSpace(opt.Username))
                settings.Authentication(new BasicAuthentication(opt.Username, opt.Password ?? string.Empty));
            return new ElasticsearchClient(settings);
        });
        services.AddScoped<IProductSearcher, ElasticsearchProductSearcher>();
        services.AddScoped<IProductSearchIndexService, ReindexProductsService>();
        services.AddScoped<IProductSearchIndexSynchronizer, ProductSearchIndexSynchronizer>();
        services.AddScoped<IOutboxEventHandler, ProductIndexOutboxEventHandler>();
        services.AddScoped<IOutboxEventHandler, OrderShippedEmailHandler>();
    }
}
