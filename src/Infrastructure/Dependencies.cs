using System.Security.Claims;
using System.Text;
using CloudinaryDotNet;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Infrastructure.Caches;
using Infrastructure.Email;
using Infrastructure.Messaging.Products;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace Infrastructure;

public static class Dependencies
{
    public static void ConfigureInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ConnectionStrings>(configuration.GetSection(nameof(ConnectionStrings)));
        services.AddDbContext<EShopContext>((sp, options) =>
        {
            options.UseNpgsql(
                    configuration.GetConnectionString(nameof(ConnectionStrings.PostgreSQL)),
                    npgsqlOptions =>
                    {
                        // Connection pool optimization
                        npgsqlOptions.MinBatchSize(1);
                        npgsqlOptions.MaxBatchSize(100);

                        // Enable connection retries
                        npgsqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorCodesToAdd: null);
                    })
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
        });

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>))
            .AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>))
            .AddScoped<IUnitOfWork, EfUnitOfWork>();

        // Redis
        var options = ConfigurationOptions.Parse(
            configuration.GetConnectionString(nameof(ConnectionStrings.Redis))!);
        options.AbortOnConnectFail = false;
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(options));
        services.AddScoped<ICacheService, RedisCacheService>();

        // Jwt
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.OptionKey));
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtService, JwtService>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwtBearerOptions =>
            {
                var jwtOptions = configuration.GetSection(JwtOptions.OptionKey).Get<JwtOptions>()!;
                jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
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
        services.AddScoped<IThirdPartyAuthService<GoogleAuthRequest, GoogleAuthResponse>, GoogleAuthService>();

        // Line Auth
        services.AddScoped<IThirdPartyAuthService<LineAuthRequest, LineAuthResponse>, LineAuthService>();

        // LinePay
        services.Configure<LinePayOptions>(configuration.GetSection(LinePayOptions.OptionKey));
        services.AddScoped<ICreatePaymentService<LinePayRequest, LinePayRequestResponse>, LinePayService>();
        services.AddScoped<IConfirmPaymentService<LinePayConfirmInput, LinePayConfirmResponse>, LinePayService>();

        // PayPal
        services.Configure<PayPalOptions>(configuration.GetSection(PayPalOptions.OptionKey));
        services.AddHttpClient<PayPalClient>((sp, client) =>
        {
            var paypalOptions = sp.GetRequiredService<IOptions<PayPalOptions>>().Value;
            client.BaseAddress = new Uri(paypalOptions.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        services.AddScoped<ICreatePaymentService<PayPalCreateOrderRequest, PayPalCreateOrderResponse>, PayPalService>();
        services.AddScoped<IConfirmPaymentService<PayPalCaptureRequest, PayPalCaptureOrderResponse>, PayPalService>();

        // kafka
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
        services.AddScoped<IProcessedEventStore, ProcessedEventStore>();
        services.AddSingleton<AdminClientConfig>(sp =>
            {
                var kafkaOptions = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;
                return new AdminClientConfig { BootstrapServers = kafkaOptions.Producer.BootstrapServers };
            })
            .AddSingleton<List<TopicSpecification>>(sp =>
            {
                var kafkaOptions = sp.GetRequiredService<IOptions<KafkaOptions>>().Value;
                var topics = new List<TopicSpecification>();
                topics.Add(
                    new TopicSpecification { Name = kafkaOptions.OutboxEventTopic, NumPartitions = 3, ReplicationFactor = 1 }
                );
                return topics;
            })
            .AddSingleton<IAdminClient>(sp =>
            {
                var config = sp.GetRequiredService<AdminClientConfig>();
                return new AdminClientBuilder(config).Build();
            });

        services
            .AddHostedService<MessageTopicInitializer>()
            .AddHostedService<OutboxPublisherHostedService>()
            .AddHostedService<OutboxConsumerHostedService>();

        // ElasticSearch
        services.Configure<ElasticsearchOptions>(configuration.GetSection(ElasticsearchOptions.OptionKey));
        services.AddSingleton(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<ElasticsearchOptions>>().Value;
            var settings = new ElasticsearchClientSettings(new Uri(opt.Url));

            if (!string.IsNullOrWhiteSpace(opt.Username))
            {
                settings.Authentication(new BasicAuthentication(opt.Username, opt.Password ?? string.Empty));
            }

            return new ElasticsearchClient(settings);
        });
        services.AddScoped<IProductSearchService, ElasticsearchProductSearchService>();
        services.AddScoped<IProductSearchIndexService, ReindexProductsService>();
        services.AddScoped<IProductSearchIndexSyncService, ProductSearchIndexSyncService>();
        services.AddScoped<IOutboxEventHandler, ProductIndexOutboxEventHandler>();
    }
}
