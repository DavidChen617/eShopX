using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using CloudinaryDotNet;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Infrastructure.Auth;
using Infrastructure.Auth.ThirdPartyAuth;
using Infrastructure.Auth.ThirdPartyAuth.Google;
using Infrastructure.Auth.ThirdPartyAuth.Google.Models;
using Infrastructure.Auth.ThirdPartyAuth.Line;
using Infrastructure.Auth.ThirdPartyAuth.Line.Models;
using Infrastructure.Caches;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Infrastructure.Email;
using Infrastructure.Image;
using Infrastructure.Logistics.EcPay;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Orders;
using Infrastructure.Messaging.Payments;
using Infrastructure.Messaging.Products;
using Infrastructure.Messaging.Shipments;
using Infrastructure.Payments;
using Infrastructure.Payments.Line;
using Infrastructure.Payments.PayPal;
using Infrastructure.Search.Elasticsearch;
using Infrastructure.Search.Embedding;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        services.Configure<GoogleAuthOptions>(configuration.GetSection(GoogleAuthOptions.OptionKey))
            .AddScoped<IThirdPartyAuthService<GoogleAuthRequest, GoogleAuthResponse>, GoogleAuthService>()
            .AddHttpClient<GoogleAuthClient>((sp, client) =>
            {
                var opt = sp.GetRequiredService<IOptions<GoogleAuthOptions>>().Value;
                client.BaseAddress = new Uri(opt.BaseUrl + "/");
            });

        // LINE Auth
        services.Configure<LineAuthOptions>(configuration.GetSection(LineAuthOptions.OptionKey))
            .AddScoped<IThirdPartyAuthService<LineAuthRequest, LineAuthResponse>, LineAuthService>()
            .AddHttpClient<LineAuthClient>((sp, client) =>
            {
                var opt = sp.GetRequiredService<IOptions<LineAuthOptions>>().Value;
                client.BaseAddress = new Uri(opt.BaseUrl + "/");
            });

        // LinePay
        services.Configure<LinePayOptions>(configuration.GetSection(LinePayOptions.OptionKey))
            .AddScoped<LinePayService>()
            .AddHttpClient<LinePayClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<LinePayOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl + "/v3/payments/");
            });

        // PayPal
        services.Configure<PayPalOptions>(configuration.GetSection(PayPalOptions.OptionKey))
            .AddScoped<PayPalService>()
            .AddHttpClient<PayPalClient>((sp, client) =>
            {
                var opt = sp.GetRequiredService<IOptions<PayPalOptions>>().Value;
                client.BaseAddress = new Uri(opt.BaseUrl);
            });
        
        services.AddScoped<IPaymentGateway, PaymentGateway>();

        // ECPay
        services.Configure<EcPayOptions>(configuration.GetSection(EcPayOptions.OptionKey))
            .AddScoped<EcPayLogisticsSelectionClient>()
            .AddScoped<EcPayCreateByTempTradeClient>()
            .AddScoped<EcPayPrintTradeDocumentClient>()
            .AddHttpClient<EcPayClient>((sp, client) =>
            {
                client.BaseAddress =
                    new Uri(sp.GetRequiredService<IOptions<EcPayOptions>>().Value.BaseUrl + "/Express/v2");
            });

        // Kafka
        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.OptionKey))
            .AddSingleton<IProducer<string, string>>(sp =>
            {
                var kafkaOptions = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;
                return new ProducerBuilder<string, string>(kafkaOptions.Producer).Build();
            })
            .AddScoped<IConsumer<string, string>>(sp =>
            {
                var kafkaOptions = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;
                return new ConsumerBuilder<string, string>(kafkaOptions.Consumer).Build();
            })
            .AddSingleton<IOutboxEventPublisher, OutboxEventPublisher>()
            .AddSingleton<AdminClientConfig>(sp =>
            {
                var kafkaOptions = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;
                return new AdminClientConfig { BootstrapServers = kafkaOptions.Producer.BootstrapServers };
            })
            .AddSingleton<List<TopicSpecification>>(sp =>
            {
                var kafkaOptions = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;
                return
                [
                    new TopicSpecification
                    {
                        Name = kafkaOptions.OutboxEventTopic, NumPartitions = 3, ReplicationFactor = 1
                    }
                ];
            })
            .AddSingleton<IAdminClient>(sp =>
                new AdminClientBuilder(sp.GetRequiredService<AdminClientConfig>()).Build());

        services
            .AddHostedService<MessageTopicInitializer>()
            .AddHostedService<OutboxPublisherHostedService>()
            .AddHostedService<OutboxConsumerHostedService>();

        // HuggingFace Embedding
        services.Configure<HuggingFaceOptions>(configuration.GetSection(HuggingFaceOptions.OptionKey))
            .AddHttpClient<IEmbeddingClient, HuggingFaceEmbeddingClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<HuggingFaceOptions>>();

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", options.Value.Token);
                client.BaseAddress = new Uri(options.Value.EmbeddingUrl);
            });

        // Elasticsearch
        services.Configure<ElasticsearchOptions>(configuration.GetSection(ElasticsearchOptions.OptionKey))
            .AddSingleton(sp =>
            {
                var opt = sp.GetRequiredService<IOptions<ElasticsearchOptions>>().Value;
                var settings = new ElasticsearchClientSettings(new Uri(opt.Url));
                if (!string.IsNullOrWhiteSpace(opt.Username))
                    settings.Authentication(new BasicAuthentication(opt.Username, opt.Password ?? string.Empty));
                return new ElasticsearchClient(settings);
            })
            .AddScoped<EsIndexInitializer>()
            .AddScoped<IProductSearcher, ElasticsearchProductSearcher>()
            .AddScoped<IProductSearchIndexService, ProductReindexer>()
            .AddScoped<IProductSearchIndexSynchronizer, ProductSearchIndexSynchronizer>()
            .AddScoped<IOutboxEventHandler, ProductIndexOutboxEventHandler>()
            .AddScoped<IOutboxEventHandler, OrderShippedEmailHandler>()
            .AddScoped<IOutboxEventHandler, PaymentPaidEmailHandler>()
            .AddScoped<IOutboxEventHandler, PaymentFailedEmailHandler>()
            .AddScoped<IOutboxEventHandler, ShipmentCompletedEmailHandler>();
    }
}
